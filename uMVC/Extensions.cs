using System;
using UnityEngine;

using Zenject;
using RektTransform;

namespace uMVC
{
    public static class Extensions
    {
        /*
        public static void ToPrefab<T>(this BinderGeneric<T> binder, String prefabLocation) where T : UnityEngine.Object {
            
            binder.ToMethod((ctx) => {
                Debug.Log("Instantiating prefab at " + prefabLocation);
                var obj = Resources.Load(prefabLocation) as GameObject;
                var component = ctx.Container.InstantiatePrefabForComponent<T>(obj);
                return component;
            });
        }

        public static void ToPrefab(this BinderUntyped binder, String prefabLocation, Type type)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                throw new Exception(string.Format("Type '{0}' is not a MonoBehaviour", type));

            binder.ToMethod(type, (ctx) => {
                Debug.Log("Starting to instantiate prefab at " + prefabLocation);
                var obj = Resources.Load(prefabLocation) as GameObject;
                Debug.Log("Loaded prefab at " + prefabLocation);
                var gameobject =  ctx.Container.InstantiatePrefabForComponent(type, obj) as MonoBehaviour;
                Debug.Log("Instantiated prefab at " + prefabLocation);
                return gameobject;
            });
        }

        public static void ToSinglePrefab<T>(this BinderGeneric<T> binder, String prefabLocation) where T : UnityEngine.Object
        {

            binder.ToSingleMethod((ctx) => {
                var obj = Resources.Load(prefabLocation) as GameObject;
                var component = ctx.Container.InstantiatePrefabForComponent<T>(obj);
                return component;
            });
        }

        public static void ToSinglePrefab(this BinderUntyped binder, String prefabLocation, Type type)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                throw new Exception(string.Format("Type '{0}' is not a MonoBehaviour", type));

            binder.ToSingleMethod((ctx) => {
                var obj = Resources.Load(prefabLocation) as GameObject;
                return ctx.Container.InstantiatePrefabForComponent(type, obj) as MonoBehaviour;
            });
        }
        */

        public static void ResetTransform(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
            t.localRotation = Quaternion.identity;
        }

        public static void ResetRectTransform(this RectTransform t)
        {
            t.ResetTransform();

            t.SetTopEdge(0f);
            t.SetLeftEdge(0f);
            t.SetRightEdge(0f);
            t.SetBottomEdge(0f);
        }

        public static void ResetTransformUnder(this Transform t, Transform parent)
        {
            t.SetParent(parent);
            t.ResetTransform();
        }

        public static void ResetRectTransformUnder(this RectTransform t, RectTransform parent)
        {
            t.SetParent(parent);
            t.ResetRectTransform();
        }

        public static void ResetTransformUnder(this MonoBehaviour m, Transform parent)
        {
            ResetTransformUnder(m.transform, parent);
        }

        public static void ResetRectTransformUnder(this MonoBehaviour m, RectTransform parent)
        {
            ResetRectTransformUnder(m.RectTransform(), parent);
        }

        public static void CopyTransform(this Transform t, Transform other)
        {
            var parent = t.parent;
            t.ResetTransformUnder(other);
            t.parent = parent;
        }

        public static RectTransform RectTransform(this Transform t)
        {
            return (RectTransform)t;
        }

        public static RectTransform RectTransform(this MonoBehaviour m)
        {
            return m.transform.RectTransform();
        }

        public static RectTransform RectTransform(this Behaviour m)
        {
            return m.transform.RectTransform();
        }
    }
}
