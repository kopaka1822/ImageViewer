using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Progress;

namespace ImageFramework.Model.Export
{
    public static class FFMpeg
    {
        public static string Path =>
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ffmpeg.exe";

        public static string ProbePath =>
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ffprobe.exe";

        public static bool IsAvailable()
        {
            return File.Exists(Path) && File.Exists(ProbePath);
        }

        public class Metadata
        {
            public string Filename;
            public int FramesPerSecond = 0; // frames per second
            public int FrameCount = 0; // total number of frames. Theoretically this could be a double but its probably not so important
            internal string FramesPerSecondString; // something like 30/1 for 30 fps (used internally by ffmpeg)
        }

        public static Metadata GetMovieMetadata(string filename)
        {
            Debug.Assert(IsAvailable());

            var metadata = new Metadata
            {
                Filename = filename,
                FrameCount = ProbeFrameCount(filename),
                FramesPerSecondString = ProbeFramesPerSecond(filename)
            };

            Debug.Assert(metadata.FramesPerSecondString != null);

            // convert frames per second string to double
            using (var dataTable = new DataTable())
            {
                var outputFps = dataTable.Compute(metadata.FramesPerSecondString, null).ToString();
                Debug.Assert(outputFps != null);
                metadata.FramesPerSecond = (int)Math.Round(double.Parse(outputFps));
            }

            return metadata;
        }
        // https://stackoverflow.com/questions/35380868/extract-frames-from-video-c-sharp
        public static async Task<TextureArray2D> ImportMovie(Metadata data, int frameStart, int numFrames, Models models)
        {
            Debug.Assert(IsAvailable());
            if (frameStart < 0 || numFrames < 1 || frameStart + numFrames > data.FrameCount)
                throw new Exception($"The combination of frameStart ({frameStart}) and numFrames ({numFrames}) is invalid for movie '{data.Filename}' with {data.FrameCount} frames.");

            // create temporary folder
            var tmpDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()));
            System.IO.Directory.CreateDirectory(tmpDir);

            try
            {
                // use ffmpeg to extract frames
                var ffmpeg = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        FileName = Path,
                        Arguments = $"-i \"{data.Filename}\" -vf \"select='between(\\n, {frameStart}, {frameStart + numFrames - 1})'\" \"{tmpDir}\\out%04d.bmp\""
                    }
                };

                if (!ffmpeg.Start())
                    throw new Exception("could not start ffmpeg.exe");


                while (!ffmpeg.HasExited)
                {
                    await Task.Run(() => ffmpeg.WaitForExit(100));

                    /*if (progress.Token.IsCancellationRequested && !ffmpeg.HasExited)
                    {
                        ffmpeg.Kill();
                        progress.Token.ThrowIfCancellationRequested();
                    }*/
                }

                Debug.Assert(ffmpeg.ExitCode == 0);
                if (ffmpeg.ExitCode != 0)
                    throw new Exception($"ffmpeg.exe exited with error code {ffmpeg.ExitCode}");

                // assume that all files have been written to tmpDir/out0001.bmp and so on
                var textures = new List<TextureArray2D>(numFrames);

                // for now load images sequentially
                for (int i = 1; i <= numFrames; i++)
                {
                    var inputFile = $"{tmpDir}\\out{i:0000}.bmp";
                    // load texture
                    var texture = await IO.LoadImageTextureAsync(inputFile, models.Progress);
                    // extract texture 2D
                    textures.Add(texture.Texture as TextureArray2D);
                }

                // convert texture array 
                return models.CombineToArray(textures);
            }
            finally
            {
                // always try to delete temporary directory
                try
                {
                    System.IO.Directory.Delete(tmpDir, true);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
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

        // use ffprobe to determine the video frame count
        private static int ProbeFrameCount(string filename)
        {
            string countFramesArgs = $"-v error -select_streams v:0 -count_frames -show_entries stream=nb_read_frames -of csv=p=0 \"{filename}\"";
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = ProbePath,
                    Arguments = countFramesArgs
                }
            };

            if (!p.Start())
                throw new Exception("could not start ffprobe.exe");

            p.WaitForExit();

            if (p.ExitCode != 0)
                throw new Exception($"ffprobe.exe exited with code {p.ExitCode}. {p.StandardError.ReadToEnd()}");

            return int.Parse(p.StandardOutput.ReadToEnd());
        }

        // use ffprobe to determine fps
        private static string ProbeFramesPerSecond(string filename)
        {
            string countFpsArgs = $"-v error -select_streams v:0 -of default=noprint_wrappers=1:nokey=1 -show_entries stream=r_frame_rate \"{filename}\"";
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = ProbePath,
                    Arguments = countFpsArgs
                }
            };

            if (!p.Start())
                throw new Exception("could not start ffprobe.exe");

            p.WaitForExit();

            if (p.ExitCode != 0)
                throw new Exception($"ffprobe.exe exited with code {p.ExitCode}. {p.StandardError.ReadToEnd()}");

            var output = p.StandardOutput.ReadLine();
            if (output == null)
                throw new Exception("ffprobe.exe did not return a valid string for frame count");

            return output.Trim();
            
        }
    }
}
