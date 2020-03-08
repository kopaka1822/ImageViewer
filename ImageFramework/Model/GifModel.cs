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
using ImageFramework.Model.Progress;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.SqlServer.Server;
using Format = SharpDX.DXGI.Format;

namespace ImageFramework.Model
{
    /// <summary>
    /// Used to create an animated diff video of two images
    /// </summary>
    /// <remarks>Exports .mp4 but is called GifModel because it was originally supposed to export gif instead of mp4</remarks>
    public class GifModel : IDisposable
    {
        private readonly GifShader shader;
        private readonly ProgressModel progressModel;

        public class Config
        {
            public int FramesPerSecond = 30;
            public int NumSeconds = 6;
            public int SliderWidth = 3;
            public string TmpFilename; // filename without extension (frames will be save in BaseFilename000 - BaseFilenameXXX)
            public string Filename; // destination filename
        }

        internal GifModel(QuadShader quad, UploadBuffer upload, ProgressModel progressModel)
        {
            this.progressModel = progressModel;
            shader = new GifShader(quad, upload);
        }

        public void CreateGif(TextureArray2D left, TextureArray2D right, Config cfg)
        {
            Debug.Assert(left != null);
            Debug.Assert(right != null);
            Debug.Assert(left.Size == right.Size);
            Debug.Assert(!progressModel.IsProcessing);

            var cts = new CancellationTokenSource();

            progressModel.AddTask(CreateGifAsync(left, right, cfg, progressModel.GetProgressInterface(cts.Token)), cts);
        }

        private async Task CreateGifAsync(TextureArray2D left, TextureArray2D right, Config cfg, IProgress progress)
        {
            // delay in milliseconds
            var numFrames = cfg.FramesPerSecond * cfg.NumSeconds;

            try
            {
                progressModel.EnableDllProgress = false;
                var leftView = left.GetSrView(LayerMipmapSlice.Mip0);
                var rightView = right.GetSrView(LayerMipmapSlice.Mip0);

                var curProg = progress.CreateSubProgress(0.9f);
                // create frames
                using (var dst = IO.CreateImage(new ImageFormat(Format.R8G8B8A8_UNorm_SRgb), left.Size, LayerMipmapCount.One))
                {
                    var dstPtr = dst.Layers[0].Mipmaps[0].Bytes;
                    var dstSize = dst.Layers[0].Mipmaps[0].Size;

                    // render frames into texture
                    using (var frame = new TextureArray2D(LayerMipmapCount.One, left.Size,
                        Format.R8G8B8A8_UNorm_SRgb, false))
                    {
                        var frameView = frame.GetRtView(LayerMipmapSlice.Mip0);

                        for (int i = 0; i < numFrames; ++i)
                        {
                            float t = (float)i / (numFrames);
                            int borderPos = (int)(t * frame.Size.Width);

                            // render frame
                            shader.Run(leftView, rightView, frameView, cfg.SliderWidth, borderPos,
                                frame.Size.Width, frame.Size.Height);

                            // save frame as png
                            frame.CopyPixels(LayerMipmapSlice.Mip0, dstPtr, dstSize);
                            var filename = $"{cfg.TmpFilename}{i:D4}";
                            await Task.Run(() =>IO.SaveImage(dst, filename, "png", GliFormat.RGBA8_SRGB), progress.Token);
                            curProg.Progress = i / (float)numFrames;
                            curProg.What = "creating frames";
                        }
                    }
                }

                // convert video
                await FFMpeg.ConvertAsync(cfg, progress.CreateSubProgress(1.0f));
            }
            finally
            {
                progressModel.EnableDllProgress = true;
            }
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
