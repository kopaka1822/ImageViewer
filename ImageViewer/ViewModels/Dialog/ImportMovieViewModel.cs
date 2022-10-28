using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.Properties;
using Xceed.Wpf.Toolkit;

namespace ImageViewer.ViewModels.Dialog
{
    public class ImportMovieViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;
        private FFMpeg.Metadata data;
        private int lastFrameCount = 0;
        private int? requiredNumFrames = null;

        public ImportMovieViewModel(ModelsEx models)
        {
            this.models = models;
            // settings to retain the configuration from the last movie
            lastFrameCount = Settings.Default.MovieLastFrameCount;
            firstFrame = Settings.Default.MovieLastFirstFrame;
            lastFrame = Settings.Default.MovieLastLastFrame;
        }

        public void Init(FFMpeg.Metadata data, int? requiredFrames)
        {
            this.data = data;
            this.requiredNumFrames = requiredFrames;
            Filename = data.Filename;
            MaxTime = TimeSpan.FromSeconds((double)(data.FrameCount + 1) / (double)data.FramesPerSecond);
            MaxFrameIndex = data.FrameCount - 1;

            // keep previous setting if similar video is imported, reset to defaults for new video
            if (lastFrameCount != data.FrameCount)
            {
                firstFrame = 0;
                lastFrame = -1;
                lastFrameCount = data.FrameCount;
            }

            // set first and last frame accordingly
            firstFrame = Utility.Clamp(firstFrame, 0, data.FrameCount - 1);
            if(lastFrame < 0) lastFrame = data.FrameCount - 1;
            lastFrame = Utility.Clamp(lastFrame, 0, data.FrameCount - 1);

            if (requiredFrames != null)
            {
                ExtraText = $"The current configuration requires {requiredFrames.Value} frames.";
            }
            else if (data.FrameCount > Device.MAX_TEXTURE_2D_ARRAY_DIMENSION)
            {
                ExtraText = $"The Image Viewer only supports videos up to {Device.MAX_TEXTURE_2D_ARRAY_DIMENSION} frames. Please select the first frame and last frame accordingly.";
            }
            else
            {
                ExtraText = ""; // no need to pay attention to anything
            }
        }

        // obtains the view model results and saves the configuration for next time
        public void GetFirstFrameAndFrameCount(out int firstFrame, out int frameCount)
        {
            // copy required values
            firstFrame = FirstFrame;
            frameCount = NumFrames;

            // remember in settings for restart
            Settings.Default.MovieLastFrameCount = data.FrameCount;
            Settings.Default.MovieLastFirstFrame = FirstFrame;
            Settings.Default.MovieLastLastFrame = LastFrame;
        }


        public string Filename { get; set; }

        public TimeSpan MinTime => TimeSpan.Zero;

        public TimeSpan MaxTime { get; set; }

        public CultureInfo Culture => ImageFramework.Model.Models.Culture;

        private int firstFrame = 0;

        public int FirstFrame
        {
            get => firstFrame;
            set
            {
                var clamped = Utility.Clamp(value, 0, data.FrameCount - 1);
                firstFrame = clamped;
                OnPropertyChanged(nameof(FirstFrame));
                OnPropertyChanged(nameof(FirstFrameTime));
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(FrameCountText));
            }
        }

        private int lastFrame = -1;

        public int LastFrame
        {
            get => lastFrame;
            set
            {
                var clamped = Utility.Clamp(value, 0, data.FrameCount - 1);
                lastFrame = clamped;
                OnPropertyChanged(nameof(LastFrame));
                OnPropertyChanged(nameof(LastFrameTime));
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(FrameCountText));
            }
        }

        private int frameSkip = 0;

        public int FrameSkip
        {
            get => frameSkip;
            set
            {
                var clamped = Math.Max(value, 0);
                frameSkip = clamped;
                OnPropertyChanged(nameof(FrameSkip));
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(FrameCountText));
            }
        }

        public double FPS => data.FramesPerSecond / (double)(FrameSkip + 1);

        public TimeSpan FirstFrameTime
        {
            get => TimeSpan.FromSeconds((double)firstFrame / (double)data.FramesPerSecond);
            set => FirstFrame = (int)Math.Round(value.TotalSeconds * data.FramesPerSecond);
        }

        public TimeSpan LastFrameTime
        {
            get => TimeSpan.FromSeconds((double)lastFrame / (double)data.FramesPerSecond);
            set => LastFrame = (int)Math.Round(value.TotalSeconds * data.FramesPerSecond);
        }

        public string FrameCountText => $"Frame Count: {NumFrames}";

        public int MaxFrameIndex { get; private set; } = 0;

        private int NumFramesUnskipped => Math.Max(lastFrame - firstFrame + 1, 0);
        public int NumFrames => (NumFramesUnskipped + FrameSkip) / (FrameSkip + 1); // <=> Math.ceil(NumFramesUnskipped/(FrameSkip+1))

        public bool IsValid => firstFrame <= lastFrame && (LastFrame - firstFrame + 1) <= Device.MAX_TEXTURE_2D_ARRAY_DIMENSION && (requiredNumFrames == null || requiredNumFrames.Value == NumFrames);

        public string ExtraText { get; private set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
