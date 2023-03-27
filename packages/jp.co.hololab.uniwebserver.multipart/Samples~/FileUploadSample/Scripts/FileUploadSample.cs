using HttpMultipartParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.UnityWebServer.Multipart.Samples
{
    public class FileUploadSample : MonoBehaviour, IHttpController
    {
        [SerializeField]
        private ImageViewer imageViewer;
      
        [Route("/")]
        public async Task<string> FileUploadPage(HttpListenerRequest request)
        {
            var uploadedFileHtml = "";
            if (request.HttpMethod == "POST")
            {
                var parser = await MultipartFormDataParser.ParseAsync(request.InputStream);

                if (parser.Files.Count >= 1)
                {
                    var file = parser.Files[0];
                    uploadedFileHtml =
                        $@"<div>
                             <p>FileName: {file.FileName}</p>
                             <p>ContentType: {file.ContentType}</p>
                             <p>FileSize: {file.Data.Length} bytes</p>
                           </div>";

                    if (file.ContentType.StartsWith("image/"))
                    {
                        _ = imageViewer.ShowImageAsync(file.Data);
                    }
                }
            }

            var html =
                $@"<html>
                     <body>
                       {uploadedFileHtml}
                       <form method = ""post"" enctype=""multipart/form-data"">
                         <input type=""file"" name=""file""/>
                         <br/>
                         <br/>
                         <input type=""submit"" value=""send""/>
                       </form>
                    </body>
                  </html>";
            return html;
        }
    }
}