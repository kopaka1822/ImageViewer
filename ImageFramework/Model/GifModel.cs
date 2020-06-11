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
            public int Width;
            public int Height;

            public void VerifyConfig()
            {
                if(Width % 2 != 0 || Height % 2 != 0)
                    throw new Exception("Image width and height have to be a multiple of 2");
            }
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

                // prepare parallel processing
                var numTasks = Environment.ProcessorCount;
                var tasks = new Task[numTasks];
                var images = new DllImageData[numTasks];
                for (int i = 0; i < numTasks; ++i)
                    images[i] = IO.CreateImage(new ImageFormat(Format.R8G8B8A8_UNorm_SRgb), left.Size,
                        LayerMipmapCount.One);

                // render frames into texture
                using (var frame = new TextureArray2D(LayerMipmapCount.One, left.Size,
                    Format.R8G8B8A8_UNorm_SRgb, false))
                {
                    var frameView = frame.GetRtView(LayerMipmapSlice.Mip0);

                    for (int i = 0; i < numFrames; ++i)
                    {
                        float t = (float) i / (numFrames);
                        int borderPos = (int) (t * frame.Size.Width);
                        int idx = i % numTasks;

                        // render frame
                        shader.Run(leftView, rightView, frameView, cfg.SliderWidth, borderPos,
                            frame.Size.Width, frame.Size.Height);

                        // copy frame from gpu to cpu
                        var dstMip = images[idx].GetMipmap(LayerMipmapSlice.Mip0);
                        var dstPtr = dstMip.Bytes;
                        var dstSize = dstMip.ByteSize;

                        // wait for previous task to finish before writing it to the file
                        if(tasks[idx] != null) await tasks[idx];

                        frame.CopyPixels(LayerMipmapSlice.Mip0, dstPtr, dstSize);
                        var filename = $"{cfg.TmpFilename}{i:D4}";

                        // write to file
                        tasks[idx] = Task.Run(() => IO.SaveImage(images[idx], filename, "png", GliFormat.RGBA8_SRGB), progress.Token);
                        
                        curProg.Progress = i / (float) numFrames;
                        curProg.What = "creating frames";
                    }
                }

                // wait for tasks to finish
                foreach (var task in tasks)
                {
                    if (task != null) await task;
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
