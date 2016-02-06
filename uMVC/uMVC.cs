using System;
using System.Collections;
using System.Collections.Generic;

using Atoms;

using Zenject;
using UnityEngine;
using ModestTree.Util;
using uMVC.Metadata;
using System.Reflection;

namespace uMVC {

    public interface IApplication
    {
        IApplication RegisterPresenter(String url, Type type);
        IApplication RegisterRoute<A>(String url) where A : MonoBehaviour;
        A SetEnv<A>();
        IApplication BackInHistoryOnEscapeButton(bool value);

        IApplication UseLayout(bool value);
        
    }



    public interface IRouter {

        IApplication app { get; }
        void Back();

        void GoTo(String url, Dictionary<string, object> parameters = null, object body = null, String layoutUrl = null, Dictionary<String, object> layoutParameters = null, object layoutBody = null, bool saveInHistory = true);
        void GoTo(IRequest request, bool saveInHistory = true);
        void ClearCurrentView();
    }

    public interface IRequest {
        String path { get; }
        Dictionary<String, object> parameters { get; }
        object body { get; }
        ILayoutRequest layoutRequest { get; set; }
        string fullPath { get; }
    }

    public interface ILayoutRequest
    {
        String path { get; set; }
        Dictionary<String, object> parameters { get; set; }
        object body { get; set; }

        string fullPath { get; }
    }

    public class Request : IRequest
    {
        public object body { get; private set; }

        public Dictionary<string, object> parameters { get; private set; }

        public string path { get; private set; }

        public ILayoutRequest layoutRequest { get; set; }
        
        public Request(string url = null, Dictionary<string, object> parameters = null, object body = null, string layoutUrl = null, Dictionary<string, object> layoutParameters = null, object layoutBody = null) {
            this.path = url;
            this.parameters = parameters != null ? parameters : new Dictionary<string, object>();
            this.body = body;

            this.layoutRequest = new LayoutRequest(layoutUrl, layoutParameters, layoutBody);
        }

        public string fullPath
        {
            get
            {
                return path != null ? "presenters/" + path : null;
            } 
        }

    }

    public class LayoutRequest : ILayoutRequest
    {
        public object body { get; set; }

        public Dictionary<string, object> parameters { get; set; }

        public string path { get; set; }
        
        public LayoutRequest(string url = null, Dictionary<string, object> parameters = null, object body = null)
        {
            this.path = url;
            this.parameters = parameters != null ? parameters : new Dictionary<string, object>();
            this.body = body;
        }

        public string fullPath
        {
            get
            {
                return path != null ? "layouts/" + path : null;
            }
        }
    }

    public interface IController
    {
        void OnDestroy();
    }

    public abstract class MVCPresenter : MonoBehaviour, IEnumLoader, IPresenter
    {
        public virtual Transform inner3D { get { return root3D; } }
        public virtual RectTransform innerUI { get { return rootUI; } }

        public virtual bool ready { get { return true; } }

        public virtual Transform root3D { get { return this.transform; } }
        public virtual RectTransform rootUI { get { return this.transform.RectTransform(); } }

        public void Load(IEnumerable e)
        {
            e.Start(this);
        }

        public abstract void OnDestroy();
        

        public void SetChild(MVCPresenter child)
        {

            child.transform.SetParent(this.transform);
            
            child.root3D.ResetTransformUnder(this.root3D);
            child.rootUI.ResetRectTransformUnder(this.innerUI);
        }
    }

    public interface IPresenter
    {
        Transform root3D { get; }
        RectTransform rootUI { get;}
        RectTransform innerUI { get; }
        Transform inner3D { get; }

        void OnDestroy();
    }

    public interface ILayoutPresenter : IPresenter {
        
    }

    public class BrokenLayoutPresenter : ILayoutPresenter
    {
        public Transform inner3D
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public RectTransform innerUI
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Transform root3D
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public RectTransform rootUI
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void OnDestroy()
        {
            throw new NotImplementedException();
        }
    }

    public interface IRoute
    {
        string path { get; set; }
        string layoutPath { get; set; }
        Type type { get; set; }
        string fullLayoutPath { get; }
        ScreenOrientation orientation { get; set; }
        string fullPath { get; }
    }

}