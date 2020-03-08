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

            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    FileName = Path,
                    Arguments = $"-framerate {config.FramesPerSecond} -i \"{config.TmpFilename}%4d.png\" -c:v libx264 -preset veryslow -crf 1 -pix_fmt yuv420p -frames:v {config.FramesPerSecond * config.NumSeconds} -r {config.FramesPerSecond} \"{config.Filename}\""
                }
            };
            var numFrames = config.NumSeconds * config.FramesPerSecond;
            progress.What = "converting";

            // progress reports
            p.ErrorDataReceived += (sender, args) =>
            {
                if(args.Data == null) return;

                if (args.Data.StartsWith("frame="))
                {
                    if (int.TryParse(args.Data.Substring("frame=".Length), out var frame))
                    {
                        progress.Progress = frame / (float) numFrames;
                    }
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
        }
    }
}
