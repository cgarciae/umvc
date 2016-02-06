using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Zenject;
using Atoms;
using ModestTree.Util;
using System.Reflection;
using uMVC.Metadata;
using JsonFx.Json;
using System.Text;

namespace uMVC {
    public abstract class App : MVCPresenter, IApplication, IRouter
    {
        public Dictionary<string, IRoute> map { get; private set; }
        private DiContainer container { get; set; }
        public bool useHistoryOnBackButton { get; private set; }
        public IApplication app { get; private set; }
        public bool useLayout { get; private set; }

        MVCPresenter current;
        MVCPresenter currentLayout;
        MVCPresenter currentPresenter;
        public Transform root;

        Stack<IRequest> history = new Stack<IRequest>();

        public void Awake()
        {
            if (root == null)
                root = this.transform;

            this.app = this;
            map = new Dictionary<string, IRoute>();

            container = new DiContainer();
            container.Bind<IApplication>().ToInstance(this);
            container.Bind<IRouter>().ToInstance(this);
            container.Bind<ILayoutPresenter>().ToInstance(new BrokenLayoutPresenter());

            SetupDI(container, map);
            
            Configure(this, container);
        }

        
        public void Start()
        {
            Init(this, container);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && useHistoryOnBackButton)
            {
                Debug.Log("Escape Pressed");
                Back();
            }
        }
        
    
        public static DiContainer SetupDI(DiContainer container, Dictionary<string, IRoute> map)
        {
            
            //Register presenters
            foreach (var tuple in GetAnnotatedTypes<Presenter>())
            {
                IRoute route = tuple.Second;
                route.type = tuple.First;
                RegisterPresenter(container, map, route);
            }

            //Register laytous
            foreach (var tuple in GetAnnotatedTypes<Layout>())
            {
                IRoute route = tuple.Second;
                route.type = tuple.First;
                RegisterPresenter(container, map, route);
            }

            //Register component
            foreach (var tuple in GetAnnotatedTypes<Element>())
            {
                IRoute route = tuple.Second;
                route.type = tuple.First;
                RegisterPresenter(container, map, route);
            }

            //Register controllers
            foreach (var tuple in GetAnnotatedTypes<Controller>())
            {
                RegisterController(container, tuple.First);
            }

            //Register services
            foreach (var tuple in GetAnnotatedTypes<Service>())
            {
                RegisterService(container, tuple.First);
            }

            return container;
        }

        public abstract void Configure(IApplication app, DiContainer container);
        public abstract void Init(IRouter router, DiContainer container);
        public IApplication RegisterPresenter(string url, Type type)
        {
            var route = new Presenter(url) {
                type = type
            };
            RegisterPresenter(container, map, route);
            return this;
        }

        public static void RegisterPresenter(DiContainer container, Dictionary<string, IRoute> presenters, IRoute route)
        {

            if (!typeof(MonoBehaviour).IsAssignableFrom(route.type))
                throw new Exception(string.Format("Type '{0}' is not a MonoBehaviour", route.type));

            
            //container.Bind(type).ToPrefab(url, type);
            container.Bind(route.type).ToTransientPrefabResource(route.fullPath);

            presenters[route.fullPath] = route;
        }

        
        public IApplication RegisterController(Type type)
        {
            RegisterController(container, type);
            return this;
        }

        public static void RegisterController(DiContainer container, Type type)
        {
            container.Bind(type).ToTransient();
        }

        public IApplication RegisterService(Type type)
        {
            RegisterService(container, type);
            return this;
        }

        public static void RegisterService(DiContainer container, Type type)
        {
            container.Bind(type).ToSingle();
        }

        public IApplication RegisterRoute<T>(DiContainer container, string url) where T : MonoBehaviour
        {
            RegisterPresenter(container, map, new Presenter(url) {
                type = typeof(T)
            });
            return this;
        }

        public void Back()
        {
            if (history.Count > 1)
            {
                var currentRequest = history.Pop();
                var previousRequest = history.Peek();
                GoTo(previousRequest, saveInHistory: false);
            }
            else
            {
                Application.Quit();
            }
        }

        public void GoTo(IRequest request, bool saveInHistory = true)
        {
            var oldView = current;
            var oldLayout = currentLayout;
            var oldPresenter = currentPresenter;

            MVCPresenter newView = null;
            MVCPresenter layout = null;
            MVCPresenter presenter = null;

            IRoute route = map[request.fullPath];

            container.Rebind<IRequest>().ToInstance(request);

            if (request.layoutRequest.path == null && route.layoutPath != null)
            {
                request.layoutRequest.path = route.layoutPath;
            }

            
            if (useLayout && request.layoutRequest.path != null)
            {
                container.Rebind<ILayoutRequest>().ToInstance(request.layoutRequest);

                Debug.Log("BEFORE RESOLVE");
                Debug.Log("Layout: " + request.layoutRequest.fullPath);
                IRoute layoutRoute = null;
                try
                {
                    layoutRoute = map[request.layoutRequest.fullPath];
                }
                catch (Exception e)
                {
                    Debug.Log("ERROR: Present Keys");
                    foreach (var key in map.Keys)
                    {
                        Debug.Log(key);
                    }
                    throw e;
                }
                var layoutType = layoutRoute.type;
                currentLayout =  layout = (MVCPresenter)container.Resolve(layoutType);
                Debug.Log("AFTER RESOLVE");

                container.Rebind<ILayoutPresenter>().ToInstance((ILayoutPresenter)layout);
            }
            else
            {
                container.Unbind<ILayoutRequest>();
            }

            Debug.Log("BEFORE RESOLVE");
            Debug.Log("View: " + request.fullPath);

            var type = route.type;
            Debug.Log(String.Format("Type: {0}", type));
            currentPresenter = presenter = (MVCPresenter)container.Resolve(type);
            Debug.Log("AFTER RESOLVE");


            if (layout != null)
            {
                Debug.Log(((ILayoutPresenter)layout).inner3D);
                Debug.Log(presenter.rootUI);
                current = newView = layout;
            }
            else
            {
                current = newView = presenter;
            }

            CorrectView(newView, layout, presenter);

            ClearView(oldView);

            if (saveInHistory)
            {
                history.Push(request);
            }

            if (route.orientation != ScreenOrientation.Unknown && route.orientation != Screen.orientation)
            {
                Atom.WaitWhile(() => !(layout == null || layout.ready) || !presenter.ready)
                .Then(() =>
                {
                    Screen.orientation = route.orientation;
                })
                .WaitFrames(1)
                .Then(() =>
                {
                    CorrectView(newView, layout, presenter);
                })
                .Start(this);
            }
        }

        public void CorrectView(MVCPresenter newView, MVCPresenter layout, MVCPresenter presenter)
        {
            if (layout != null)
            {
                layout.SetChild(presenter);
            }

            newView.ResetRectTransformUnder(root.RectTransform());
        }

        public void GoTo(String url, Dictionary<String, object> parameters = null, object body = null, String layoutUrl = "master", Dictionary<String, object> layoutParameters = null, object layoutBody = null, bool saveInHistory = true)
        {
            var request = new Request(url, parameters, body, layoutUrl, layoutParameters, layoutBody);
            GoTo(request, saveInHistory);
        }

        public void ClearCurrentView()
        {
            ClearView(current);
        }

        private void ClearView(MonoBehaviour view)
        {
            Atom.WaitFrames(1).Then(() =>
            {
                if (view != null)
                    Destroy(view.gameObject);
            })
            .Start(this);
        }

        public static IEnumerable<Tuple<Type, MetadataType>> GetAnnotatedTypes<MetadataType>() where MetadataType : Attribute
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (var route in type.GetCustomAttributes(true))
                {
                    if (route is MetadataType)
                        yield return Tuple.New(type, route as MetadataType);
                }
            }
        }

        public A SetEnv<A>()
        {
            return SetEnv<A>(container);
        }

        public static A SetEnv<A>(DiContainer container)
        {
            var textAsset = (TextAsset)Resources.Load("config");
            var map = Deserialize<Dictionary<string, object>>(textAsset.text);

            var env_type = (string)map["__env__"];
            var general_env = (Dictionary<string, object>)map["__general__"];
            var current_env = (Dictionary<string, object>)map[env_type];

            foreach (var item in current_env)
            {
                general_env[item.Key] = item.Value;
            }

            A env = Cast<A>(general_env);

            container.Bind<A>().ToInstance(env);

            return env;
        }

        public static A Deserialize<A>(string s)
        {
            JsonReaderSettings readerSettings = new JsonReaderSettings();
            readerSettings.TypeHintName = "__type__";
            return new JsonReader(s, readerSettings).Deserialize<A>();
        }

        public static object Deserialize(string s, Type type)
        {
            JsonReaderSettings readerSettings = new JsonReaderSettings();
            readerSettings.TypeHintName = "__type__";
            return new JsonReader(s, readerSettings).Deserialize(type);
        }

        public static IEnumerable<A> DeserializeList<A>(string s)
        {
            JsonReaderSettings readerSettings = new JsonReaderSettings();
            readerSettings.TypeHintName = "__type__";
            JsonReader reader = new JsonReader(s, readerSettings);
            return new JsonReader(s, readerSettings).Deserialize<A[]>();
        }

        public static string Serialize<A>(A a)
        {
            JsonWriterSettings writerSettings = new JsonWriterSettings();
            writerSettings.TypeHintName = "__type__";
            StringBuilder json = new StringBuilder();
            JsonWriter writer = new JsonWriter(json, writerSettings);
            writer.Write(a);
            return json.ToString();
        }

        public static A Cast<A>(object b)
        {
            return Deserialize<A>(Serialize(b));
        }

        object Cast(object b, Type type)
        {
            return Deserialize(Serialize(b), type);
        }

        
        public IApplication RegisterRoute<A>(string url) where A : MonoBehaviour
        {
            return RegisterRoute<A>(container, url);
        }

        public IApplication BackInHistoryOnEscapeButton(bool value)
        {
            this.useHistoryOnBackButton = value;
            return this;
        }

        public IApplication UseLayout(bool value)
        {
            this.useLayout = value;
            return this;
        }
    }
}
