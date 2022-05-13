using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Models.Display;

namespace ImageViewer.ViewModels.Display
{
    public class MovieViewModel : INotifyPropertyChanged, IDisposable
    {
        private ModelsEx models;
        private bool repeatVideo = false;
        private bool playVideo = false;

        public MovieViewModel(ModelsEx models)
        {
            this.models = models;
            models.Display.PropertyChanged += DisplayOnPropertyChanged;

            PreviousCommand = new ActionCommand(PreviousFrame);
            NextCommand = new ActionCommand(NextFrame);
            ToggleRepeatCommand = new ActionCommand(() =>
            {
                repeatVideo = !repeatVideo;
                OnPropertyChanged(nameof(RepeatVideo));
            });
            PlayCommand = new ActionCommand(PlayPause);
            StopCommand = new ActionCommand(Stop);
        }

        public void Dispose()
        {
            // unsubscribe
            models.Display.PropertyChanged -= DisplayOnPropertyChanged;
        }

        public void PlayPause()
        {
            // TODO set play to false when finished playing
            // TODO set active layer to 0 if play was false and current layer is last layer
            playVideo = !playVideo;
            OnPropertyChanged(nameof(IsPlaying));
        }

        public void Stop()
        {
            playVideo = false;
            models.Display.ActiveLayer = 0;
            OnPropertyChanged(nameof(IsPlaying));
        }

        public void PreviousFrame()
        {
            var prev = models.Display.ActiveLayer - 1;
            if (prev < 0)
            {
                if (!repeatVideo) return;
                // set to last frame
                prev = models.Images.NumLayers - 1;
            }

            models.Display.ActiveLayer = prev;
        }

        public void NextFrame()
        {
            var next = models.Display.ActiveLayer + 1;
            if (next >= models.Images.NumLayers)
            {
                if (!repeatVideo) return;
                // set to first frame
                next = 0;
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
                    break;
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand ToggleRepeatCommand { get; }

        public bool RepeatVideo => repeatVideo;

        public bool IsPlaying => playVideo;

        private int framesPerSecond = 24;
        public string FPS
        {
            get => framesPerSecond.ToString(ImageFramework.Model.Models.Culture);
            set
            {
                int parsedFps = 0;
                var converted = int.TryParse(value, out parsedFps);
                if (converted)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (parsedFps == framesPerSecond) return;


                    framesPerSecond = parsedFps;
                }

                OnPropertyChanged(nameof(FPS));
                OnPropertyChanged(nameof(TickFrequency));
            }
        }

        // this can assumed to be constant, since this view will be removed and destroyed when the image changes
        public int MaxFrameId => models.Images.NumLayers - 1;
        public int TickFrequency => framesPerSecond;

        public int TickValue
        {
            get => models.Display.ActiveLayer;
            set
            {
                if (value == models.Display.ActiveLayer) return;
                models.Display.ActiveLayer = value;
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

        



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
