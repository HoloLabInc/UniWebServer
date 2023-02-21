using System;

namespace HoloLab.UnityWebServer
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RouteAttribute : Attribute
    {
        public string Path;

        public RouteAttribute(string path)
        {
            Path = path;
        }
    }
}