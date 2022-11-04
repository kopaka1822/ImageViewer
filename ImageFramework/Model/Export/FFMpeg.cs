using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Progress;
using ImageFramework.Utility;
using Format = SharpDX.DXGI.Format;

namespace ImageFramework.Model.Export
{
    public static class FFMpeg
    {
        public class FFMpegUnavailableException : Exception
        {
            internal FFMpegUnavailableException() : base("could not locate ffmpeg.exe or ffprobe.exe") {}
        }

        public static string Path =>
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ffmpeg.exe";

        public static string ProbePath =>
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ffprobe.exe";

        public static bool IsAvailable()
        {
            return File.Exists(Path) && File.Exists(ProbePath);
        }

        public static void CheckAvailable()
        {
            if (!File.Exists(Path) || !File.Exists(ProbePath))
                throw new FFMpegUnavailableException();
        }

        public class Metadata
        {
            public string Filename;
            public float FramesPerSecond = 0; // frames per second
            public int FrameCount = 0; // total number of frames.
            public int Width = 0; // width of a frame
            public int Height = 0; // height of a frame
            internal string FramesPerSecondString; // something like 30/1 for 30 fps (used internally by ffmpeg)
        }

        public enum Preset
        {
            ultrafast = 0,
            superfast,
            veryfast,
            faster,
            fast,
            medium, // = default
            slow,
            slower,
            veryslow
        }

        public class MovieExportConfig
        {
            public string Filename;
            public int FramesPerSecond; // this is an integer because otherwise we need to determine the fraction (1.5 is an invalid framerate, it needs to be specified as 3/2)
            public TextureArray2D Source;
            public int FirstFrame;
            public int FrameCount;
            public Preset Preset = Preset.medium;
            public float Multiplier = 1.0f; // color multiplier from export model

            public void Verify()
            {
                if (Source == null)
                    throw new NullReferenceException("Image source must be valid");
                if (FirstFrame < 0 || FirstFrame >= Source.NumLayers)
                    throw new Exception("Invalid value for FirstFrame");
                if (FrameCount <= 0 || FirstFrame + FrameCount > Source.NumLayers)
                    throw new Exception("Invalid value for FrameCount");
                if (FramesPerSecond <= 0)
                    throw new Exception("Invalid value for frames per second");
                if (String.IsNullOrEmpty(Filename))
                    throw new NullReferenceException("Filename is empty");
            }
        }

        public static Metadata GetMovieMetadata(string filename)
        {
            Debug.Assert(IsAvailable());
            CheckAvailable();

            var metadata = new Metadata
            {
                Filename = filename,
                FrameCount = ProbeFrameCount(filename)
            };

            FillMetadataWidthHeightFps(metadata);

            Debug.Assert(metadata.FramesPerSecondString != null);

            // convert frames per second string to double
            using (var dataTable = new DataTable())
            {
                var outputFps = dataTable.Compute(metadata.FramesPerSecondString, null).ToString();
                Debug.Assert(outputFps != null);
                metadata.FramesPerSecond = (float)double.Parse(outputFps);
            }

            return metadata;
        }

        public static async Task<Metadata> GetMovieMetadataAsync(string filename, Models models)
        {
            var cts = new CancellationTokenSource();
            var task = Task.Run(() =>
            {
                models.Progress.GetProgressInterface(cts.Token).What = "counting frames";
                return GetMovieMetadata(filename);
            }, cts.Token);
            models.Progress.AddTask(task, cts, false);
            await models.Progress.WaitForTaskAsync();
            return task.Result;
        }

        internal static async Task<TextureArray2D> ImportMovie(Metadata data, int frameStart, int frameSkip, int numFrames, SharedModel shared, IProgress progress)
        {
            Debug.Assert(IsAvailable());
            CheckAvailable();

            var lastFrameIndex = frameStart + (numFrames - 1) * (frameSkip + 1);
            if (frameStart < 0 || numFrames < 1 || lastFrameIndex >= data.FrameCount)
                throw new Exception($"The combination of frameStart ({frameStart}), numFrames ({numFrames}) and frameSkip ({frameSkip}) is invalid for movie '{data.Filename}' with {data.FrameCount} frames.");

            // reserve memory for the result texture
            var resultTex = new TextureArray2D(new LayerMipmapCount(numFrames, 1), new Size3(data.Width, data.Height),
                Format.R8G8B8A8_UNorm_SRgb, false);

            // create temporary folder
            var tmpDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()));
            System.IO.Directory.CreateDirectory(tmpDir);

            // create as many tasks as available threads to load the images in parallel
            var numThreads = Environment.ProcessorCount;
            var threadTasks = new Task<DllImageData>[numThreads];

            try
            {
                // use ffmpeg to extract all frames at once (with a single command)
                var ffmpeg = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        FileName = Path,
                        Arguments = $"-i \"{data.Filename}\" -vf \"select='between(\\n, {frameStart}, {lastFrameIndex})'\" -vsync passthrough \"{tmpDir}\\out%04d.bmp\""
                    }
                };

                var prog1 = progress.CreateSubProgress(0.5f);
                string errors = "";
                ffmpeg.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data == null) return;

                    Console.Error.WriteLine("FFMPEG: " + args.Data);

                    if (args.Data.StartsWith("frame="))
                    {
                        var substr = args.Data.Substring("frame=".Length);
                        substr = substr.TrimStart().Split(' ')[0];
                        if (int.TryParse(substr, out var frame))
                        {
                            prog1.Progress = (frame - frameStart) / (float)(lastFrameIndex - frameStart + 1);
                        }
                    }
                    else if (args.Data.StartsWith("error", StringComparison.OrdinalIgnoreCase))
                    {
                        errors += args.Data;
                        errors += "\n";
                    }
                };

                if (!ffmpeg.Start())
                    throw new Exception("could not start ffmpeg.exe");
                ffmpeg.BeginErrorReadLine();



                while (!ffmpeg.HasExited)
                {
                    await Task.Run(() => ffmpeg.WaitForExit(100));
                    prog1.What = "decoding frames to disc";

                    if (progress.Token.IsCancellationRequested && !ffmpeg.HasExited)
                    {
                        ffmpeg.Kill();
                        progress.Token.ThrowIfCancellationRequested();
                    }
                }

                Debug.Assert(ffmpeg.ExitCode == 0);
                if (ffmpeg.ExitCode != 0)
                {
                    if (!String.IsNullOrEmpty(errors))
                        throw new Exception(errors);
                    throw new Exception($"ffmpeg.exe exited with error code {ffmpeg.ExitCode}");
                }

                // assume that all files have been written to tmpDir/out0001.bmp and so on
                var prog2 = progress.CreateSubProgress(1.0f);
                prog2.What = "importing frames from disc";

                int curLayer = 0;
                // for now load images sequentially
                for (int i = 1; i <= numFrames + numThreads; i++)
                {
                    var iAfterSkip = 1 + (i - 1) * (frameSkip + 1);
                    var inputFile = $"{tmpDir}\\out{iAfterSkip:0000}.bmp";
                    var threadIdx = i % numThreads;

                    // 2.  wait for previous task to finish before opening a new image
                    // the loop repeats itself for numThreads more times in order to wait for all tasks
                    if (threadTasks[threadIdx] != null)
                    {
                        // create directX texture on main thread (it is potentially not thread safe)
                        var dllData = await threadTasks[threadIdx];
                        using (var frameTex = new TextureArray2D(dllData))
                        {
                            // copy layer to target texture
                            shared.Convert.CopyLayer(frameTex, LayerMipmapSlice.Mip0, resultTex, new LayerMipmapSlice(curLayer++, 0));
                        }
                        dllData.Dispose();
                        threadTasks[threadIdx] = null;
                    }

                    // 1. start a new task on thread index
                    if (i <= numFrames)
                    {
                        threadTasks[threadIdx] = Task.Run(() => IO.LoadImage(inputFile), progress.Token);
                        prog2.Progress = i / (float)numFrames;
                    }

                    progress.Token.ThrowIfCancellationRequested();
                }

                // all task should be finished since we iterate an additional numThread times in the previous loop
                Debug.Assert(threadTasks.All(task => task == null));

                // store result text into savedResult, because resultTex will be disposed in the finally statement
                var savedResult = resultTex;
                resultTex = null;
                return savedResult;
            }
            finally
            {
                // terminate all running threads
                foreach (var t in threadTasks)
                {
                    try
                    {
                        if (t != null)
                        {
                            var dllData = await t;
                            dllData.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                // delete all intermediate textures
                resultTex?.Dispose();

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

        /// <summary>
        /// enqueues movie import into the progress model
        /// </summary>
        /// <param name="data"></param>
        /// <param name="frameStart">index of first frame</param>
        /// <param name="frameSkip">number of frames that will be skipped after each frame</param>
        /// <param name="numFrames">total number of frames</param>
        /// <param name="models"></param>
        /// <returns></returns>
        public static async Task<TextureArray2D> ImportMovieAsync(Metadata data, int frameStart, int frameSkip, int numFrames, Models models)
        {
            var cts = new CancellationTokenSource();
            var task = ImportMovie(data, frameStart, frameSkip, numFrames, models.SharedModel,
                models.Progress.GetProgressInterface(cts.Token));
            models.Progress.AddTask(task, cts, false);
            await models.Progress.WaitForTaskAsync();
            return task.Result;
        }

        internal static async Task ExportMovie(MovieExportConfig config, IProgress progress, Models models)
        {
            Debug.Assert(IsAvailable());
            CheckAvailable();
            config.Verify();

            // delete old video file first if it exists, otherwise ffmpeg will hang
            if (File.Exists(config.Filename))
                File.Delete(config.Filename);

            // create temp directory
            var tmpDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()));
            System.IO.Directory.CreateDirectory(tmpDir);

            var exportTasks = new List<Task>();

            try
            {
                // first half is export to disc
                progress.What = "writing frames to disc";
                var progress1 = progress.CreateSubProgress(0.5f);

                // export selected frames to tmp directory
                for (int i = 0; i < config.FrameCount; ++i)
                {
                    // TODO add option for cropping?
                    var task = models.Export.ExportAsync(
                        new ExportDescription(config.Source, $"{tmpDir}\\out{(i + 1):0000}", "bmp")
                        {
                            Layer = config.FirstFrame + i,
                            Mipmap = 0,
                            FileFormat = GliFormat.RGB8_SRGB,
                            Overlay = models.Overlay.Overlay,
                            Multiplier = config.Multiplier
                        }.ForceAlignment(2, 2), progress.Token);

                    exportTasks.Add(task);
                    progress1.Progress = (float)i / (float)config.FrameCount;
                    progress.Token.ThrowIfCancellationRequested();
                }

                // wait for all tasks to finish
                foreach (var task in exportTasks)
                {
                    await task;
                    progress.Token.ThrowIfCancellationRequested();
                }

                // second half is video creation
                var progress2 = progress.CreateSubProgress(1.0f);
                progress.What = "converting frames to video";

                // create video from exported frames
                var exportArgs =
                    $"-r {config.FramesPerSecond} -i \"{tmpDir}\\out%04d.bmp\" -c:v libx264 -preset {config.Preset} -crf 12 -vf \"fps={config.FramesPerSecond},format=yuv420p\" \"{config.Filename}\"";

                var p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        FileName = Path,
                        Arguments = exportArgs
                    }
                };

                // progress reports
                string errors = "";
                p.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data == null) return;
                    Console.Error.WriteLine("FFMPEG: " + args.Data);

                    if (args.Data.StartsWith("frame="))
                    {
                        var substr = args.Data.Substring("frame=".Length);
                        substr = substr.TrimStart().Split(' ')[0];
                        if (int.TryParse(substr, out var frame))
                        {
                            progress2.Progress = frame / (float)config.FrameCount;
                        }
                    }
                    else if (args.Data.StartsWith("error", StringComparison.OrdinalIgnoreCase))
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

                if (!String.IsNullOrEmpty(errors))
                    throw new Exception(errors);
            }
            finally
            {
                // wait for all tasks to finish
                foreach (var task in exportTasks)
                {
                    try
                    {
                        if(task != null) await task;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                try
                {
                    // delete tmpDir directory
                    Directory.Delete(tmpDir, true);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static async Task ExportMovieAsync(MovieExportConfig config, Models models)
        {
            var cts = new CancellationTokenSource();
            var task = ExportMovie(config, models.Progress.GetProgressInterface(cts.Token), models);
            models.Progress.AddTask(task, cts, false);
            await models.Progress.WaitForTaskAsync();
        }

        internal static async Task ConvertAsync(GifModel.Config config, IProgress progress, int numFrames)
        {
            Debug.Assert(IsAvailable());
            CheckAvailable();

            if(File.Exists(config.Filename))
                File.Delete(config.Filename);

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

        private static readonly char[] trimChars = new char[] { ',', '\n', ' ', '\r' };

        // use ffprobe to determine the video frame count
        private static int ProbeFrameCount(string filename)
        {
            //string countFramesArgs = $"-v error -select_streams v:0 -count_frames -show_entries stream=nb_read_frames -of csv=p=0 \"{filename}\"";
            string countFramesArgs = $"-v error -select_streams v:0 -count_packets -show_entries stream=nb_read_packets -of csv=p=0 \"{filename}\"";
            
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

            var output = p.StandardOutput.ReadToEnd();
            output = output.Trim(trimChars);
            return int.Parse(output);
        }

        // use ffprobe to determine fps
        //private static string ProbeFramesPerSecond(string filename)
        private static void FillMetadataWidthHeightFps(Metadata meta)
        {
            string countFpsArgs = $"-v error -select_streams v:0 -of default=noprint_wrappers=1 -show_entries stream=r_frame_rate,width,height \"{meta.Filename}\"";
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

            string line;
            while ((line = p.StandardOutput.ReadLine()) != null)
            {
                if (line.StartsWith("width="))
                {
                    var widthStr = line.Substring("width=".Length);
                    int.TryParse(widthStr, out meta.Width);
                }
                else if (line.StartsWith("height="))
                {
                    var heightStr = line.Substring("height=".Length);
                    int.TryParse(heightStr, out meta.Height);
                }
                else if (line.StartsWith("r_frame_rate="))
                {
                    meta.FramesPerSecondString = line.Substring("r_frame_rate=".Length).Trim();
                }
            }

            if(String.IsNullOrEmpty(meta.FramesPerSecondString))
                throw new Exception("ffprobe.exe did not return a valid string for frame count");
            if(meta.Width <= 0)
                throw new Exception("ffprobe.exe did not return a valid string for frame width");
            if (meta.Height <= 0)
                throw new Exception("ffprobe.exe did not return a valid string for frame height");
        }

        //private static IReadOnlyList<string> s_formats = null;
        private static HashSet<string> s_formats;
        public static HashSet<string> Formats()
        {
            if (s_formats == null)
            {
                s_formats = new HashSet<string>
                {
                    "mp4",
                    "mov",
                    "mpeg",
                    "avi",
                    "flv",
                    "webm",
                    "mpeg",
                    "mkv",
                    "wmv",
                    "gif"
                };
            }
            return s_formats;
        }
    }
}
