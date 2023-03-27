using System.Collections.Generic;
using System.Reflection;

namespace HoloLab.UniWebServer
{
    internal class ControllerMethod
    {
        public IHttpController Controller;
        public List<Parameter> Parameters;
        public MethodInfo MethodInfo;
        public MethodInfo CheckMethod;
    }
}