using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;

namespace HoloLab.UnityWebServer
{
    public class HttpServerComponent : MonoBehaviour
    {
        [SerializeField]
        private int port = 8080;

        [SerializeField]
        private List<GameObject> controllers = new List<GameObject>();

        [SerializeField]
        private List<StaticRouteSetting> staticRouteSettings = new List<StaticRouteSetting>();

        private HttpServer httpServer;
        private Router router;

        private void Awake()
        {
            httpServer = new HttpServer();

            router = new Router()
            {
                Context = SynchronizationContext.Current
            };

            foreach (var controller in controllers)
            {
                var httpControllers = controller.GetComponentsInChildren<IHttpController>();
                foreach (var httpController in httpControllers)
                {
                    router.AddController(httpController);
                }
            }

            // Add static route
            foreach (var staticRouteSetting in staticRouteSettings)
            {
                var staticPageController = new StaticPageController(staticRouteSetting);
                router.AddControllerMethod(staticPageController.GetControllerMethod());
            }

            httpServer.OnRequest += HttpServer_OnRequest;
        }

        private async void HttpServer_OnRequest(HttpListenerContext context)
        {
            await router.Route(context.Request, context.Response);
        }

        private void OnEnable()
        {
            httpServer.Start(port);
        }

        private void OnDisable()
        {
            httpServer.Stop();
        }
    }
}
