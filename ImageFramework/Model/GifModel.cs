using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
using SharpDX.DirectWrite;
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
            public string Label1; // name of the first image
            public string Label2; // name of the second image
        }

        internal GifModel(ProgressModel progressModel)
        {
            this.progressModel = progressModel;
            shader = new GifShader();
        }

        public void CreateGif(TextureArray2D left, TextureArray2D right, Config cfg, SharedModel shared)
        {
            Debug.Assert(left != null);
            Debug.Assert(right != null);
            Debug.Assert(left.Size == right.Size);
            Debug.Assert(!progressModel.IsProcessing);

            var cts = new CancellationTokenSource();

            progressModel.AddTask(CreateGifAsync(left, right, cfg, progressModel.GetProgressInterface(cts.Token), shared), cts);
        }

        private async Task CreateGifAsync(TextureArray2D left, TextureArray2D right, Config cfg, IProgress progress, SharedModel shared)
        {
            // delay in milliseconds
            var numFrames = cfg.FramesPerSecond * cfg.NumSeconds;
         
            // size compatible?
            bool disposeImages = false;
            if ((left.Size.Width % 2) != 0 || (left.Size.Height % 2) != 0)
            {
                disposeImages = true;
                var pad = Size3.Zero;
                pad.X = left.Size.Width % 2;
                pad.Y = left.Size.Height % 2;
                left = (TextureArray2D)shared.Padding.Run(left, Size3.Zero, pad, PaddingShader.FillMode.Clamp, null, shared, false);
                right = (TextureArray2D)shared.Padding.Run(right, Size3.Zero, pad, PaddingShader.FillMode.Clamp, null, shared, false);
                Debug.Assert(left.Size.Width % 2 == 0 && left.Size.Height % 2 == 0);
            }

            try
            {
                progressModel.EnableDllProgress = false;
                var leftView = left.GetSrView(LayerMipmapSlice.Mip0);
                var rightView = right.GetSrView(LayerMipmapSlice.Mip0);

                var curProg = progress.CreateSubProgress(0.9f);

                // prepare parallel processing
                var numTasks = Environment.ProcessorCount;
                var tasks = new Task[numTasks];
                var images = new DllImageData[numTasks];
                for (int i = 0; i < numTasks; ++i)
                    images[i] = IO.CreateImage(new ImageFormat(Format.R8G8B8A8_UNorm_SRgb), left.Size,
                        LayerMipmapCount.One);
                int textSize = left.Size.Y / 18;
                float padding = textSize / 4.0f;

                // render frames into texture
                using (var frame = new TextureArray2D(LayerMipmapCount.One, left.Size,
                    Format.R8G8B8A8_UNorm_SRgb, false))
                {
                    var frameView = frame.GetRtView(LayerMipmapSlice.Mip0);
                    using (var d2d = new Direct2D(frame))
                    {
                        for (int i = 0; i < numFrames; ++i)
                        {
                            float t = (float)i / (numFrames);
                            int borderPos = (int)(t * frame.Size.Width);
                            int idx = i % numTasks;

                            // render frame
                            shader.Run(leftView, rightView, frameView, cfg.SliderWidth, borderPos,
                                frame.Size.Width, frame.Size.Height, shared.QuadShader, shared.Upload);

                            // add text
                            using (var c = d2d.Begin())
                            {
                                var halfX = left.Size.X / 2;
                                c.Text(new Float2(padding), new Float2(halfX - padding, left.Size.Y - padding), textSize, Colors.White, cfg.Label1);

                                c.Text(new Float2(halfX + padding, padding), new Float2(left.Size.X - padding, left.Size.Y - padding), textSize, Colors.White, cfg.Label2, TextAlignment.Trailing);
                            }

                            // copy frame from gpu to cpu
                            var dstMip = images[idx].GetMipmap(LayerMipmapSlice.Mip0);
                            var dstPtr = dstMip.Bytes;
                            var dstSize = dstMip.ByteSize;

                            // wait for previous task to finish before writing it to the file
                            if (tasks[idx] != null) await tasks[idx];

                            frame.CopyPixels(LayerMipmapSlice.Mip0, dstPtr, dstSize);
                            var filename = $"{cfg.TmpFilename}{i:D4}";

                            // write to file
                            tasks[idx] = Task.Run(() =>
                            {
                                try
                                {
                                    IO.SaveImage(images[idx], filename, "png", GliFormat.RGBA8_SRGB);
                                }
                                catch (Exception)
                                {
                                    // ignored (probably cancelled by user)
                                }
                            }, progress.Token);

                            curProg.Progress = i / (float)numFrames;
                            curProg.What = "creating frames";

                            progress.Token.ThrowIfCancellationRequested();
                        }
                    }
                }

                // wait for tasks to finish
                for (var i = 0; i < tasks.Length; i++)
                {
                    if (tasks[i] != null) await tasks[i];
                    tasks[i] = null;
                }

                // convert video
                await FFMpeg.ConvertAsync(cfg, progress.CreateSubProgress(1.0f));
            }
            finally
            {
                progressModel.EnableDllProgress = true;

                if(disposeImages)
                {
                    left.Dispose();
                    right.Dispose();
                }
            }
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
