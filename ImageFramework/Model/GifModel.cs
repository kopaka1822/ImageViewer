using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using Microsoft.SqlServer.Server;
using Format = SharpDX.DXGI.Format;

namespace ImageFramework.Model
{
    /// <summary>
    /// temporarily removed due to bad results
    /// </summary>
    public class GifModel : IDisposable
    {
        private readonly GifShader shader;

        public class Config
        {
            public int FramesPerSecond = 30;
            public int NumSeconds = 6;
            public int SliderWidth = 3;
            public string Filename;
        }

        internal GifModel(QuadShader quad)
        {
            shader = new GifShader(quad);
        }

        public void CreateGif(TextureArray2D left, TextureArray2D right, Config cfg, int layer = 0, int mipmap = 0)
        {
            //Debug.Assert(left.Width == right.Width);
            //Debug.Assert(left.Height == right.Height);
            throw new NotImplementedException();
            // delay in milliseconds
            /*int delay = 1000 / cfg.FramesPerSecond;
            var img = new Bitmap(left.GetWidth(mipmap), left.GetHeight(mipmap));
            var lockRect = new Rectangle(0, 0, img.Width, img.Height);
            var bytesPerFrame = (uint)(img.Width * img.Height * 4);

            var numImages = cfg.FramesPerSecond * cfg.NumSeconds;

            var leftView = left.GetSrView(layer, mipmap);
            var rightView = right.GetSrView(layer, mipmap);

            using (var frame = new TextureArray2D(1, 1, left.GetWidth(mipmap), left.GetHeight(mipmap),
                Format.B8G8R8A8_UNorm_SRgb, false))
            {
                var frameView = frame.GetRtView(0, 0);

                using (var gif = AnimatedGif.AnimatedGif.Create(cfg.Filename, delay))
                {
                    for (int i = 0; i < numImages; ++i)
                    {
                        float t = (float) i / (numImages);
                        int borderPos = (int)(t * frame.Width);

                        // render frame
                        shader.Run(leftView, rightView, frameView, cfg.SliderWidth, borderPos,
                            frame.Width, frame.Height);

                        // put frame into image
                        var bytes = frame.GetBytes(0, 0, bytesPerFrame);
                        var bitData = img.LockBits(lockRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        Marshal.Copy(bytes, 0, bitData.Scan0, (int)bytesPerFrame);
                        img.UnlockBits(bitData);

                        // add frame
                        gif.AddFrame(img, quality: GifQuality.Bit8);
                    }
                }
            }*/
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
