using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        public MovieViewModel(ModelsEx models)
        {
            this.models = models;
            models.Display.PropertyChanged += DisplayOnPropertyChanged;

            PreviousCommand = new ActionCommand(PreviousFrame);
            NextCommand = new ActionCommand(NextFrame);
        }

        public void Dispose()
        {
            // unsubscribe
            models.Display.PropertyChanged -= DisplayOnPropertyChanged;
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
                    break;
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand ToggleRepeatCommand { get; }



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
