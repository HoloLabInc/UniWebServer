using System.Collections;
using System.Collections.Generic;

namespace HoloLab.UniWebServer
{
    internal enum ParameterType
    {
        Static = 0,
        DefaultPage,
        String,
        Int,
        Float,
        Double,
    }

    internal struct Parameter
    {
        public ParameterType Type;
        public string Name;
        public bool IncludeSlash;
        public int ArgumentIndex;

        public override string ToString()
        {
            return $"Type: {Type}, Name: {Name}, IncludeSlash: {IncludeSlash}, ArgumentIndex: {ArgumentIndex}";
        }
    }
}