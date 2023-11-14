using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using System.Windows.Threading;

namespace ImageViewer.ViewModels.Display
{
    public class MovieViewModel : INotifyPropertyChanged, IDisposable
    {


        private ModelsEx models;
        private bool playVideo = false;
        private DispatcherTimer clock = new DispatcherTimer(DispatcherPriority.Render);
        private DateTime lastTickStamp;
        private bool mirrorForward = true; // mirror replay direction is forward

        public MovieViewModel(ModelsEx models, MovieDisplayModel baseModel)
        {
            this.models = models;
            models.Display.PropertyChanged += DisplayOnPropertyChanged;
            models.Settings.PropertyChanged += SettingsOnPropertyChanged;
            baseModel.OnKeyFunc = OnKeyDown;

            PreviousCommand = new ActionCommand(PreviousFrame);
            NextCommand = new ActionCommand(NextFrame);
            PlayCommand = new ActionCommand(PlayPause);
            StopCommand = new ActionCommand(Stop);

            ReplayModes = new List<ListItemViewModel<SettingsModel.MovieRepeatMode>>
            {
                new ListItemViewModel<SettingsModel.MovieRepeatMode>
                {
                    Cargo = SettingsModel.MovieRepeatMode.NoRepeat,
                    Name = "Once"
                },
                new ListItemViewModel<SettingsModel.MovieRepeatMode>
                {
                    Cargo = SettingsModel.MovieRepeatMode.Repeat,
                    Name = "Repeat"
                },
                new ListItemViewModel<SettingsModel.MovieRepeatMode>
                {
                    Cargo = SettingsModel.MovieRepeatMode.Mirror,
                    Name = "Mirror"
                }
            };
            selectedReplayMode = ReplayModes.Find(model => model.Cargo == models.Settings.MovieRepeat);

            lastTickStamp = DateTime.Now;
            clock.Tick += ClockOnTick;
            OnFpsChanged();
        }

        private bool OnKeyDown(Key key)
        {
            switch (key)
            {
                case Key.OemComma:
                    PreviousFrame();
                    break;
                case Key.OemPeriod:
                    NextFrame();
                    break;
                case Key.Space:
                    PlayPause();
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsModel.MovieFps):
                    OnFpsChanged();
                    OnPropertyChanged(nameof(FPS));
                    OnPropertyChanged(nameof(TickFrequency));
                    OnPropertyChanged(nameof(TimeText));
                    break;
                case nameof(SettingsModel.MovieRepeat):
                    SelectedReplayMode = ReplayModes.Find(model => model.Cargo == models.Settings.MovieRepeat);
                    break;
            }
        }

        public void Dispose()
        {
            // unsubscribe
            models.Display.PropertyChanged -= DisplayOnPropertyChanged;
            models.Settings.PropertyChanged -= SettingsOnPropertyChanged;
            clock.Stop();
        }

        public void PlayPause()
        {
            // restart with first frame if playback stopped at last frame
            if (!IsPlaying && models.Display.ActiveLayer == MaxFrameId)
            {
                models.Display.ActiveLayer = 0;
            }

            IsPlaying = !IsPlaying;
        }

        public void Stop()
        {
            IsPlaying = false;
            models.Display.ActiveLayer = 0;
        }

        public void PreviousFrame()
        {
            IsPlaying = false; // stop playback for this feature

            var prev = models.Display.ActiveLayer - 1;
            if (prev < 0)
            {
                if (models.Settings.MovieRepeat != SettingsModel.MovieRepeatMode.Repeat) return;
                // set to last frame
                prev = models.Images.NumLayers - 1;
            }

            models.Display.ActiveLayer = prev;
        }

        public void NextFrame()
        {
            IsPlaying = false; // stop playback for this feature

            var next = models.Display.ActiveLayer + 1;
            if (next >= models.Images.NumLayers)
            {
                if (models.Settings.MovieRepeat != SettingsModel.MovieRepeatMode.Repeat) return;
                // set to first frame
                next = 0;
            }
            models.Display.ActiveLayer = next;
        }

        public void AdvanceFrames(int numFrames)
        {
            if (models.Settings.MovieRepeat == SettingsModel.MovieRepeatMode.Mirror && !mirrorForward)
                numFrames = -numFrames; // advance in negative direction

            var next = models.Display.ActiveLayer + numFrames;

            if (next >= models.Images.NumLayers)
            {
                switch (models.Settings.MovieRepeat)
                {
                    case SettingsModel.MovieRepeatMode.NoRepeat:
                        models.Display.ActiveLayer = MaxFrameId; // set last frame
                        return;
                    case SettingsModel.MovieRepeatMode.Repeat:
                        models.Display.ActiveLayer = 0; // set to first frame
                        lastTickStamp = DateTime.Now; // reset fractional frame count
                        return;
                    case SettingsModel.MovieRepeatMode.Mirror:
                        models.Display.ActiveLayer = MaxFrameId; // mirror at the end, set to last frame
                        mirrorForward = false;
                        return;
                }
            }

            if (next < 0)
            {
                next = 0;
                if (models.Settings.MovieRepeat == SettingsModel.MovieRepeatMode.Mirror)
                    mirrorForward = true; // mirror in 
            }

            models.Display.ActiveLayer = next;
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.ActiveLayer):
                    OnPropertyChanged(nameof(FrameID));
                    OnPropertyChanged(nameof(TickValue));
                    OnPropertyChanged(nameof(TimeText));
                    break;
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }

        public bool IsPlaying
        {
            get => playVideo;
            set
            {
                if (playVideo == value) return;

                Debug.Assert(clock != null);
                if (value)
                {
                    // start timer
                    lastTickStamp = DateTime.Now;
                    clock.Start();
                }
                else
                {
                    // stop timer
                    clock.Stop();
                }

                playVideo = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        public string FPS
        {
            //get => models.Settings.MovieFps.ToString("F2", ImageFramework.Model.Models.Culture);
            get => Math.Round((decimal)models.Settings.MovieFps, 2).ToString(ImageFramework.Model.Models.Culture);
            set
            {
                float parsedFps = 0.0f;
                var converted = float.TryParse(value, NumberStyles.Number, ImageFramework.Model.Models.Culture, out parsedFps);
                //var converted = float.TryParse(value, parsedFps);
                if (converted)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (parsedFps == models.Settings.MovieFps) return;

                    models.Settings.MovieFps = parsedFps;
                }

                // always invoke this (to reset invalid user entries)
                OnPropertyChanged(nameof(FPS));
            }
        }

        // this can assumed to be constant, since this view will be removed and destroyed when the image changes
        public int MaxFrameId => models.Images.NumLayers - 1;
        public float TickFrequency => models.Settings.MovieFps;

        public int TickValue
        {
            get => models.Display.ActiveLayer;
            set
            {
                if (value == models.Display.ActiveLayer) return;
                models.Display.ActiveLayer = value;
            }
        }

        public string TimeText
        {
            get
            {
                int seconds = (int)Math.Floor(models.Display.ActiveLayer / models.Settings.MovieFps);
                return $"{(seconds / 60):00}:{(seconds % 60):00}";
            }
        }

        public string FrameID
        {
            get => models.Display.ActiveLayer.ToString(ImageFramework.Model.Models.Culture);
            set
            {
                int parsedFrameID = 0;
                var converted = int.TryParse(value, out parsedFrameID);
                if (converted)
                {
                    // clamp converted value to valid range
                    var clamped = Utility.Clamp(parsedFrameID, 0, models.Images.NumLayers - 1);

                    models.Display.ActiveLayer = clamped;
                }

                OnPropertyChanged(nameof(FrameID));
            }
        }

        private void OnFpsChanged()
        {
            //clock.Interval = TimeSpan.FromMilliseconds(1000.0f / models.Settings.MovieFps);
            // use half the interval because the clock is sometimes too early
            clock.Interval = TimeSpan.FromMilliseconds(0.5f * 1000.0f / models.Settings.MovieFps);
        }

        private void ClockOnTick(object sender, EventArgs e)
        {
            // calculate time that actually passed
            var timeElapsed = (DateTime.Now - lastTickStamp).TotalSeconds;
            // convert to frames passed
            var fps = (double)models.Settings.MovieFps;
            var passedFrames = (int)Math.Floor(timeElapsed * fps);
            // advance tick stamp by actual passed seconds (accumulate the overdue seconds for next frame)
            lastTickStamp = lastTickStamp.AddSeconds((double)passedFrames / fps);

            AdvanceFrames(passedFrames);

            // stop playback if we reached the last frame (for non repeat mode)
            if (models.Display.ActiveLayer == MaxFrameId && models.Settings.MovieRepeat == SettingsModel.MovieRepeatMode.NoRepeat)
            {
                IsPlaying = false;
            }
        }

        public List<ListItemViewModel<SettingsModel.MovieRepeatMode>> ReplayModes { get; }

        private ListItemViewModel<SettingsModel.MovieRepeatMode> selectedReplayMode = null;

        public ListItemViewModel<SettingsModel.MovieRepeatMode> SelectedReplayMode
        {
            get => selectedReplayMode;
            set
            {
                if (value == null || value == selectedReplayMode) return;
                selectedReplayMode = value;
                OnPropertyChanged(nameof(SelectedReplayMode));
                models.Settings.MovieRepeat = value.Cargo;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
