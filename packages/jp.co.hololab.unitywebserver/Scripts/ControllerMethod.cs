using System.Collections.Generic;
using System.Reflection;

namespace HoloLab.UnityWebServer
{
    internal class ControllerMethod
    {
        public IHttpController Controller;
        public List<Parameter> Parameters;
        public MethodInfo MethodInfo;
        public MethodInfo CheckMethod;
    }
}