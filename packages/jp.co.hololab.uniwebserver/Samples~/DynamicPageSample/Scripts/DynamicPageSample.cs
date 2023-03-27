using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.UniWebServer.Samples
{
    public class DynamicPageSample : MonoBehaviour, IHttpController
    {
        [Route("/")]
        public string SamplePages()
        {
            var samplePages = new List<string>()
            {
                "/samplepage/number/1",
                "/samplepage/text/samplepageText",
                "/samplepage/path/foo/bar",
                "/samplepage/form",
                "/samplepage/query?text=Hello%20world!&id=1",
                "/samplepage/filedownload"
            };

            string links = "";
            foreach (var samplePage in samplePages)
            {
                links += $@"<p><a href=""{samplePage}""> {samplePage} </a></p>";
            }

            var html =
                $@"<html>
                      <body>
                          <div>
                              {links}
                          </div>
                      </body>
                  </html>";

            return html;

        }

        [Route("samplepage/number/:number/")]
        public string GetNumberSample(int number)
        {
            Debug.Log("number: " + number);
            return "number: " + number;
        }

        [Route("samplepage/text/:text")]
        public string GetTextSample(string text)
        {
            Debug.Log("text: " + text);
            return "text: " + text;
        }

        [Route("samplepage/path/::path")]
        public string GetPathSample(string path)
        {
            Debug.Log("path: " + path);
            return "path: " + path;
        }

        [Route("samplepage/form")]
        public async Task<string> FormSample(HttpListenerRequest request)
        {
            string postData = "";
            Debug.Log(request.HttpMethod);
            Debug.Log(request.ContentType);

            if (request.HttpMethod == "POST")
            {
                var reader = new StreamReader(request.InputStream);
                string body = await reader.ReadToEndAsync();

                // TODO parse body
                // application/x-www-form-urlencoded
                postData = body;
            }

            var html =
                $@"<html>
                      <body>
                          <div>
                            Post data: {postData}
                          </div>
                          <form method = ""post"">
                            <input type=""hidden"" name=""hidden"" value=""hidden_text""/>
                            <input type=""text"" name=""text""/>
                            <input type=""submit"" value=""send""/>
                          </form>
                      </body>
                  </html>";

            return html;
        }

        [Route("samplepage/query")]
        public string QuerySample(HttpListenerRequest request)
        {
            var builder = new StringBuilder(); ;
            foreach (var key in request.QueryString.AllKeys)
            {
                var value = request.QueryString[key];
                builder.Append($"<p>key: {key}, value: {value}</p>");
            }

            var html =
                $@"<html>
                      <body>
                          {builder}
                      </body>
                  </html>";

            return html;
        }

        [Route("samplepage/filedownload")]
        public byte[] FileDownload(HttpListenerResponse response)
        {
            response.AppendHeader("Content-Disposition", "attachment; filename=\"SampleFile.txt\"");

            var text = "This is sample file.";
            return Encoding.UTF8.GetBytes(text);
        }
    }
}