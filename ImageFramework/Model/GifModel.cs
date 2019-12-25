using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Export;
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
        private readonly ProgressModel progress;

        public class Config
        {
            public int FramesPerSecond = 30;
            public int NumSeconds = 6;
            public int SliderWidth = 3;
            public string TmpFilename; // filename without extension (frames will be save in BaseFilename000 - BaseFilenameXXX)
            public string Filename; // destination filename
        }

        internal GifModel(QuadShader quad, UploadBuffer upload, ProgressModel progress)
        {
            this.progress = progress;
            shader = new GifShader(quad, upload);
        }

        public void CreateGif(TextureArray2D left, TextureArray2D right, Config cfg)
        {
            Debug.Assert(left != null);
            Debug.Assert(right != null);
            Debug.Assert(left.Size == right.Size);
            Debug.Assert(!progress.IsProcessing);

            var cts = new CancellationTokenSource();

            progress.AddTask(CreateGifAsync(left, right, cfg, cts.Token), cts);
        }

        private async Task CreateGifAsync(TextureArray2D left, TextureArray2D right, Config cfg, CancellationToken ct)
        {
            // delay in milliseconds
            var numImages = cfg.FramesPerSecond * cfg.NumSeconds;

            try
            {
                progress.EnableDllProgress = false;
                var leftView = left.GetSrView(0, 0);
                var rightView = right.GetSrView(0, 0);

                // create frames
                using (var dst = IO.CreateImage(new ImageFormat(Format.R8G8B8A8_UNorm_SRgb), left.Size, 1, 1))
                {
                    var dstPtr = dst.Layers[0].Mipmaps[0].Bytes;
                    var dstSize = dst.Layers[0].Mipmaps[0].Size;

                    using (var frame = new TextureArray2D(1, 1, left.Size,
                        Format.R8G8B8A8_UNorm_SRgb, false))
                    {
                        var frameView = frame.GetRtView(0, 0);

                        for (int i = 0; i < numImages; ++i)
                        {
                            float t = (float)i / (numImages);
                            int borderPos = (int)(t * frame.Size.Width);

                            // render frame
                            shader.Run(leftView, rightView, frameView, cfg.SliderWidth, borderPos,
                                frame.Size.Width, frame.Size.Height);

                            // save frame as png
                            frame.CopyPixels(0, 0, dstPtr, dstSize);
                            var filename = $"{cfg.TmpFilename}{i:D4}";
                            await Task.Run(() =>IO.SaveImage(dst, filename, "png", GliFormat.RGBA8_SRGB), ct);
                            progress.Progress = i / (float)numImages;
                            progress.What = "creating frames";
                        }
                    }
                }

                // convert video
                await FFMpeg.ConvertAsync(cfg, ct);
            }
            finally
            {
                progress.EnableDllProgress = true;
            }
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
