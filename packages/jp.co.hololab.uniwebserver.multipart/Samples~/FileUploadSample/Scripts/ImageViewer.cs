using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.UniWebServer.Multipart.Samples
{
    public class ImageViewer : MonoBehaviour
    {
        private Texture2D texture;

        private void Awake()
        {
            texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);

            var renderer = GetComponent<Renderer>();
            renderer.material.mainTexture = texture;
        }

        public async Task ShowImageAsync(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                var data = ms.ToArray();
                texture.LoadImage(data);

                var aspectRatio = (float)texture.width / texture.height;

                var scale = transform.localScale;
                scale.x = scale.y * aspectRatio;
                transform.localScale = scale;
            }
        }
    }
}