using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HoloLab.UniWebServer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    internal class TestController
    {
        [Route("path1/path2")]
        public void StaticRouting()
        {
        }

        [Route("")]
        public void DefaultRouting1()
        {
        }

        [Route("/")]
        public void DefaultRouting2()
        {
        }
    }

    public class RouterHelperTest
    {
        private static Parameter StaticParameter(string name)
        {
            return new Parameter
            {
                Type = ParameterType.Static,
                Name = name
            };
        }

        private static Parameter DefaultPageParameter()
        {
            return new Parameter
            {
                Type = ParameterType.DefaultPage
            };
        }

        [Test]
        public void TryParseMapAttributeTest()
        {
            var testCases = new List<(string methodName, List<Parameter> parameters)>
            {
                (
                    nameof(TestController.StaticRouting),
                    new List<Parameter>
                    {
                        StaticParameter("path1"),
                        StaticParameter("path2"),
                    }
                ),
                (
                    nameof(TestController.DefaultRouting1),
                    new List<Parameter>
                    {
                        DefaultPageParameter()
                    }
                ),
                (
                    nameof(TestController.DefaultRouting2),
                    new List<Parameter>
                    {
                        DefaultPageParameter()
                    }
                )
            };

            foreach (var testCase in testCases)
            {
                var (method, route) = GetMethodInfoAndRoute(typeof(TestController), testCase.methodName);

                var result = RouterHelper.TryParseMapAttribute(method, route, out var parameters);

                Assert.That(result, Is.True);
                Assert.That(parameters, Is.EqualTo(testCase.parameters));
            }
        }

        private (MethodInfo method, RouteAttribute route) GetMethodInfoAndRoute(Type controllerType, string methodName)
        {
            var method = typeof(TestController).GetMethod(methodName);
            var route = method.GetCustomAttributes(true)
                .FirstOrDefault(attr => attr is RouteAttribute) as RouteAttribute;
            if (route == null)
            {
                Assert.Fail("RouteAttribute not found");
            }

            return (method, route);
        }

        [Test]
        public void GetStringParametersTest()
        {
            var testCases = new List<(Uri Uri, List<string> StringParameters)>
            {
                (
                    new Uri("http://localhost"),
                    new List<string>
                    {
                        "/"
                    }
                ),
                (
                    new Uri("http://localhost/"),
                    new List<string>
                    {
                        "/"
                    }
                ),
                (
                    new Uri("http://localhost/test"),
                    new List<string>
                    {
                        "test"
                    }
                ),
                (
                    new Uri("http://localhost/test/"),
                    new List<string>
                    {
                        "test",
                        "/"
                    }
                ),
            };

            foreach (var testCase in testCases)
            {
                var parameters = RouterHelper.GetStringParameters(testCase.Uri);
                Assert.That(parameters, Is.EqualTo(testCase.StringParameters));
            }
        }

        [Test]
        public void MatchParameterTest()
        {
            var testCases = new List<(string StrParameter, Parameter Parameter, object Argument)>
            {
                (
                    "path",
                    new Parameter()
                    {
                        Type = ParameterType.Static,
                        Name = "path"
                    },
                    null
                ),
                (
                    "/",
                    new Parameter()
                    {
                        Type = ParameterType.DefaultPage
                    },
                    null
                ),
                (
                    "pathString",
                    new Parameter()
                    {
                        Type = ParameterType.String
                    },
                    "pathString"
                ),
                (
                    "10",
                    new Parameter()
                    {
                        Type = ParameterType.Int
                    },
                    10
                ),
            };

            foreach (var testCase in testCases)
            {
                var result = RouterHelper.MatchParameter(testCase.StrParameter, testCase.Parameter, out var argument);
                Assert.That(result, Is.True);
                Assert.That(argument, Is.EqualTo(testCase.Argument));
            }
        }

        [Test]
        public void MatchRoutingTest()
        {
            var testCases = new List<(string methodName, List<string> parameters)>
            {
                (
                    nameof(TestController.StaticRouting),
                    new List<string>
                    {
                        "path1",
                        "path2",
                    }
                ),
                (
                    nameof(TestController.DefaultRouting1),
                    new List<string>
                    {
                        "/"
                    }
                ),
                (
                    nameof(TestController.DefaultRouting2),
                    new List<string>
                    {
                        "/"
                    }
                )
            };

            foreach (var testCase in testCases)
            {
                var (method, route) = GetMethodInfoAndRoute(typeof(TestController), testCase.methodName);
                RouterHelper.TryParseMapAttribute(method, route, out var parameters);

                var controllerMethod = new ControllerMethod()
                {
                    MethodInfo = method,
                    Parameters = parameters
                };
                var result = RouterHelper.MatchRouting(controllerMethod, testCase.parameters, out var arguments);
                Assert.That(result, Is.True);
            }
        }
    }
}