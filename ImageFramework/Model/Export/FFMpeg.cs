using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public static async Task ConvertAsync(GifModel.Config config, CancellationToken ct)
        {
            Debug.Assert(IsAvailable());

            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = Path,
                    Arguments = $"-framerate {config.FramesPerSecond} -i \"{config.TmpFilename}%4d.png\" -c:v libx264 -preset veryslow -crf 1 -pix_fmt yuv420p -frames:v {config.FramesPerSecond * config.NumSeconds} -r {config.FramesPerSecond} \"{config.Filename}\""
                }
            };

            p.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null) return;
                Console.Out.WriteLine("Outpur: " + args.Data);
            };
            p.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data == null) return;
                Console.Out.WriteLine("Error: " + args.Data);
            };

            if (!p.Start())
                throw new Exception("could not start ffmpeg.exe");

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            while (!p.HasExited)
            {
                await Task.Run(() => p.WaitForExit(100));

                if (ct.IsCancellationRequested)
                {
                    p.Kill();
                    ct.ThrowIfCancellationRequested();
                }
            }
        }
    }
}
