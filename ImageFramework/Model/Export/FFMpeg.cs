using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Model.Progress;

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

        internal static async Task ConvertAsync(GifModel.Config config, IProgress progress)
        {
            Debug.Assert(IsAvailable());

            string startArgs =
                $"-framerate {config.FramesPerSecond} -i \"{config.TmpFilename}%4d.png\" -c:v libx264 -preset veryslow -crf 12 -pix_fmt yuv420p -frames:v {config.FramesPerSecond * config.NumSeconds} -r {config.FramesPerSecond} \"{config.Filename}\"";

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
            var numFrames = config.NumSeconds * config.FramesPerSecond;
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
