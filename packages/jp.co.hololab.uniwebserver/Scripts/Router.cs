using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.UniWebServer
{
    public class Router
    {
        private List<ControllerMethod> controllerMethods = new List<ControllerMethod>();

        public SynchronizationContext Context { set; get; } = null;

        public void AddController(IHttpController controller)
        {
            var controllerType = controller.GetType();
            foreach (var method in controllerType.GetMethods())
            {
                var route = method.GetCustomAttributes(true)
                    .FirstOrDefault(attr => attr is RouteAttribute) as RouteAttribute;
                if (route == null)
                {
                    continue;
                }

                if (RouterHelper.TryParseMapAttribute(method, route, out var parameters))
                {
                    MethodInfo checkMethod = null;

                    var controllerMethod = new ControllerMethod()
                    {
                        Controller = controller,
                        MethodInfo = method,
                        CheckMethod = checkMethod,
                        Parameters = parameters
                    };
                    controllerMethods.Add(controllerMethod);
                }
            }
        }

        internal void AddControllerMethod(ControllerMethod controllerMethod)
        {
            controllerMethods.Add(controllerMethod);
        }

        public Task Route(HttpListenerRequest request, HttpListenerResponse response)
        {
            var uri = request.Url;
            var strParams = RouterHelper.GetStringParameters(uri);

            foreach (var controllerMethod in controllerMethods)
            {
                if (!RouterHelper.MatchRouting(controllerMethod, strParams, out var arguments))
                {
                    continue;
                }

                var method = controllerMethod.MethodInfo;
                var methodParameters = method.GetParameters();

                // set request/response arguments
                for (var i = 0; i < methodParameters.Length; i++)
                {
                    var p = methodParameters[i];
                    if (p.ParameterType == typeof(HttpListenerRequest))
                    {
                        arguments[i] = request;
                    }
                    else if (p.ParameterType == typeof(HttpListenerResponse))
                    {
                        arguments[i] = response;
                    }
                }

                // invoke controller method
                var controller = controllerMethod.Controller;
                Context.Post(async _ =>
                {
                    var result = method.Invoke(controller, arguments);
                    await Task.Run(async () => { await WriteResponse(response, method, result); });
                }, null);
                return Task.CompletedTask;
            }

            response.StatusCode = 404;
            response.Close();
            return Task.CompletedTask;
        }

        private async Task<bool> WriteResponse(HttpListenerResponse response, MethodInfo method, object result)
        {
            var success = false;

            var returnType = method.ReturnType;

            try
            {
                // Await task result
                if (returnType == typeof(Task<string>))
                {
                    var resultTask = (Task<string>)result;
                    result = await resultTask;
                    returnType = typeof(string);
                }
                else if (returnType == typeof(Task<byte[]>))
                {
                    var resultTask = (Task<byte[]>)result;
                    result = await resultTask;
                    returnType = typeof(byte[]);
                }
                else if (returnType == typeof(Task))
                {
                    var resultTask = (Task)result;
                    await resultTask;
                    returnType = typeof(void);
                }

                if (returnType == typeof(void))
                {
                    return true;
                }

                // Convert result to byte array
                byte[] data;

                if (returnType == typeof(byte[]))
                {
                    data = result as byte[];
                }
                else if (returnType == typeof(string))
                {
                    data = StringToBytes((string)result);
                }
                else
                {
                    var json = JsonUtility.ToJson(result);
                    data = StringToBytes(json);
                }

                if (response.OutputStream.CanWrite)
                {
                    if (data != null)
                    {
                        await response.OutputStream.WriteAsync(data, 0, data.Length);
                        success = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);

                // Internal server error
                response.StatusCode = 500;
            }
            finally
            {
                try
                {
                    response.Close();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            return success;
        }

        private static byte[] StringToBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
    }
}