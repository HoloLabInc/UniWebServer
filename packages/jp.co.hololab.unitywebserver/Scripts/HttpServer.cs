using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HoloLab.UnityWebServer
{
    public class HttpServer
    {
        private HttpListener httpListener;
        private CancellationTokenSource tokenSource;

        public event Action<HttpListenerContext> OnRequest;

        public void Start(int port)
        {
            httpListener = new HttpListener();
            var uri = $"http://*:{port}/";
            httpListener.Prefixes.Add(uri);

            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            Task.Run(async () =>
            {
                httpListener.Start();
                while (true)
                {
                    var context = await httpListener.GetContextAsync();
                    OnRequest?.Invoke(context);
                }
            }, token);
        }

        public void Stop()
        {
            tokenSource.Cancel();
            httpListener.Stop();
        }
    }
}