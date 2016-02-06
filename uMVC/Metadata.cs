

using System;
using UnityEngine;

namespace uMVC.Metadata
{
    public class Presenter : Attribute, IRoute
    {
        
        public Type type { get; set; }
        public ScreenOrientation orientation { get; set; }

        public string path { get; set; }
        public string layoutPath { get; set; }

        public Presenter(string path, string layoutPath = "master", ScreenOrientation orientation = ScreenOrientation.Unknown)
        {
            this.path = path;
            this.layoutPath = layoutPath;
            this.orientation = orientation;
        }

        public string fullPath {
            get
            {
                return path != null ? "presenters/" + path : null;
            }
        }

        public string fullLayoutPath
        {
            get
            {
                return layoutPath != null ? "layouts/" + layoutPath : null;
            }
        }
    }

    public class Layout : Attribute, IRoute
    {
        public string path { get; set; }
        public string layoutPath { get; set; }
        public Type type { get; set; }
        public string fullLayoutPath { get; set; }
        public ScreenOrientation orientation { get; set; }
        
        public Layout(string path)
        {
            this.path = path;
        }

        public string fullPath
        {
            get
            {
                return path != null ? "layouts/" + path : null;
            }
        }
    }

    public class Element : Attribute, IRoute
    {
        public string fullPath { get; set; }
        public Type type { get; set; }
        public string fullLayoutPath { get; set; }
        public ScreenOrientation orientation { get; set; }
        public string path { get; set; }
        public string layoutPath { get; set; }
        public Element(string path)
        {
            this.fullPath = "elements/" + path;
        }
    }

    class Controller : Attribute
    {
        public Controller(){}
    }

    class Service : Attribute
    {
        public Service() { }
    }
}
