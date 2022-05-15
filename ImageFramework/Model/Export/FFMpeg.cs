using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Progress;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace ImageFramework.Model.Export
{
    public static class FFMpeg
    {
        public static string Path =>
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ffmpeg.exe";

        public static bool IsAvailable()
        {
            return File.Exists(Path);
        }

        public class Metadata
        {
            public string Filename;
            public int FramesPerSecond = 0; // frames per second
            public int FrameCount = 0; // total number of frames

            internal MediaFile File;
        }

        internal static Metadata GetMovieMetadata(string filename)
        {
            Debug.Assert(IsAvailable());

            var file = new MediaFile(filename);
            Debug.Assert(file != null);

            using (var engine = new Engine(Path))
            {
                engine.GetMetadata(file); // writes to file.Metadata
            }
            
            return new Metadata
            {
                Filename = filename,
                FramesPerSecond = (int)Math.Round(file.Metadata.VideoData.Fps),
                FrameCount = (int)Math.Round(file.Metadata.Duration.TotalSeconds * file.Metadata.VideoData.Fps),
                File = file
            };
        }
        // https://stackoverflow.com/questions/35380868/extract-frames-from-video-c-sharp
        internal static async Task<TextureArray2D> ImportMovie(Metadata data, IProgress progress)
        {
            Debug.Assert(IsAvailable());
            Debug.Assert(data.File != null);

            // create temporary folder
            var tmpDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()));
            System.IO.Directory.CreateDirectory(tmpDir);


            progress.What = "Writing Frames";
            //progress.Token.IsCancellationRequested
            using(var engine = new Engine(Path))
            {
                for(int frame = 0; frame < data.FrameCount; ++frame)
                {
                    var tmpFilename = $"{tmpDir}\\frame{frame:D4}.png";
                    var outputFile = new MediaFile(tmpFilename);
                    var options = new ConversionOptions
                    {
                        Seek = TimeSpan.FromSeconds((float)(frame) / (float)data.FramesPerSecond)
                    };
                    // TODO do this async?
                    //engine.GetThumbnail(data.File, outputFile, options);
                    await Task.Run(() => engine.GetThumbnail(data.File, outputFile, options));

                    progress.Progress = (float)frame / (float)data.FrameCount;
                    progress.Token.ThrowIfCancellationRequested();
                }
            }

            TextureArray2D res = null;



            return res;
        }

        internal static async Task ConvertAsync(GifModel.Config config, IProgress progress, int numFrames)
        {
            Debug.Assert(IsAvailable());

            string startArgs =
                $"-r {config.FramesPerSecond} -f concat -safe 0 -i \"{config.TmpDirectory}\\files.txt\" -c:v libx264 -preset veryslow -crf 12 -vf \"fps={config.FramesPerSecond},format=yuv420p\" \"{config.Filename}\"";

            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    FileName = Path,
                    Arguments = startArgs
                }
            };
            progress.What = "converting";

            // progress reports
            string errors = "";
            p.ErrorDataReceived += (sender, args) =>
            {
                if(args.Data == null) return;
                Console.Error.WriteLine("FFMPEG: " + args.Data);

                if (args.Data.StartsWith("frame="))
                {
                    var substr = args.Data.Substring("frame=".Length);
                    substr = substr.TrimStart().Split(' ')[0];
                    if (int.TryParse(substr, out var frame))
                    {
                        progress.Progress = frame / (float) numFrames;
                    }
                } 
                else if(args.Data.StartsWith("error", StringComparison.OrdinalIgnoreCase))
                {
                    errors += args.Data;
                    errors += "\n";
                }
            };

            if (!p.Start())
                throw new Exception("could not start ffmpeg.exe");

            p.BeginErrorReadLine();

            while (!p.HasExited)
            {
                await Task.Run(() => p.WaitForExit(100));

                if (progress.Token.IsCancellationRequested && !p.HasExited)
                {
                    p.Kill();
                    progress.Token.ThrowIfCancellationRequested();
                }
            }

            if(!String.IsNullOrEmpty(errors))
                throw new Exception(errors);
        }
    }
}
