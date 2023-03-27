using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HoloLab.UniWebServer
{
    internal static class RouterHelper
    {
        public static bool TryParseMapAttribute(MethodInfo method, RouteAttribute route, out List<Parameter> parameters)
        {
            var mapParameters = route.Path.Split('/')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
            var methodParameters = method.GetParameters();

            parameters = new List<Parameter>();

            // If only "" or "/", route to default page
            if (mapParameters.Length == 0)
            {
                var defaultPageParameter = new Parameter()
                {
                    Type = ParameterType.DefaultPage
                };
                parameters.Add(defaultPageParameter);
                return true;
            }

            for (var i = 0; i < mapParameters.Length; i++)
            {
                var mapParameter = mapParameters[i];

                var name = mapParameter.TrimStart(':');
                var parameter = new Parameter
                {
                    Name = name,
                    IncludeSlash = false
                };

                if (mapParameter.StartsWith(":"))
                {
                    var methodParam = methodParameters.FirstOrDefault(x => x.Name == name);
                    if (methodParam == null)
                    {
                        var warning = $"Argument {name} not found in {method.Name}";
                        Debug.LogWarning(warning);
                        return false;
                    }

                    parameter.ArgumentIndex = methodParam.Position;
                    var type = methodParam.ParameterType;

                    if (TryConvertTypeToParameterType(type, out var parameterType))
                    {
                        parameter.Type = parameterType;
                    }
                    else
                    {
                        var warning = $"Type of argument {name} not supported";
                        Debug.LogWarning(warning);
                        return false;
                    }

                    // If parameter starts with ::, all subsequent paths are taken as an argument
                    if (mapParameter.StartsWith("::"))
                    {
                        if (i != mapParameters.Length - 1)
                        {
                            var warning = ":: parameter can be used at end";
                            Debug.LogWarning(warning);
                            return false;
                        }

                        if (parameterType != ParameterType.String)
                        {
                            var warning = $"{name} should be string";
                            Debug.LogWarning(warning);
                            return false;
                        }

                        parameter.IncludeSlash = true;
                    }
                }
                else
                {
                    parameter.Type = ParameterType.Static;
                }

                parameters.Add(parameter);
            }

            return true;
        }

        public static List<string> GetStringParameters(Uri uri)
        {
            var segments = uri.Segments.Skip(1).ToArray();

            var strParams = segments
                .Select(s => s.Replace("/", ""))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            if (segments.Length == 0 || segments.LastOrDefault()?.EndsWith("/") == true)
            {
                strParams.Add("/");
            }

            return strParams;
        }

        public static bool MatchRouting(ControllerMethod controllerMethod, IReadOnlyList<string> strParams,
            out object[] arguments)
        {
            var parameters = controllerMethod.Parameters;
            arguments = null;

            // No match if routing parameters are more than url parameters
            if (strParams.Count < parameters.Count)
            {
                return false;
            }

            if (strParams.Count > parameters.Count)
            {
                var lastParameter = parameters.LastOrDefault();

                // If the last parameter does not get a path containing a slash, the number of parameters does not match
                if (!lastParameter.IncludeSlash)
                {
                    return false;
                }
            }

            var method = controllerMethod.MethodInfo;
            var methodParameters = method.GetParameters();
            arguments = new object[methodParameters.Length];

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var strParam = strParams[i];

                if (MatchParameter(strParam, parameter, out var obj))
                {
                    if (parameter.IncludeSlash)
                    {
                        obj = string.Join("/", strParams.Skip(i));
                    }

                    if (parameter.Type != ParameterType.Static && parameter.Type != ParameterType.DefaultPage)
                    {
                        arguments[parameter.ArgumentIndex] = obj;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public static bool MatchParameter(string strParameter, Parameter parameter, out object obj)
        {
            obj = null;
            bool result;
            switch (parameter.Type)
            {
                case ParameterType.Static:
                    return strParameter == parameter.Name;
                case ParameterType.DefaultPage:
                    return strParameter == "/";
                case ParameterType.String:
                    obj = strParameter;
                    return true;
                case ParameterType.Int:
                    result = int.TryParse(strParameter, out var intValue);
                    if (result)
                    {
                        obj = intValue;
                    }

                    return result;
                case ParameterType.Float:
                    result = float.TryParse(strParameter, out var floatValue);
                    if (result)
                    {
                        obj = floatValue;
                    }

                    return result;
                case ParameterType.Double:
                    result = double.TryParse(strParameter, out var doubleValue);
                    if (result)
                    {
                        obj = doubleValue;
                    }

                    return result;
            }

            return false;
        }

        private static bool TryConvertTypeToParameterType(Type type, out ParameterType parameterType)
        {
            if (type == typeof(string))
            {
                parameterType = ParameterType.String;
                return true;
            }

            if (type == typeof(int))
            {
                parameterType = ParameterType.Int;
                return true;
            }

            if (type == typeof(float))
            {
                parameterType = ParameterType.Float;
                return true;
            }

            if (type == typeof(double))
            {
                parameterType = ParameterType.Double;
                return true;
            }

            parameterType = ParameterType.Static;
            return false;
        }
    }
}