using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using ImageViewer.Models;

namespace ImageViewer.ViewModels.Dialog
{
    public class ExportMovieViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;

        public ExportMovieViewModel(ModelsEx models, string filename)
        {
            Filename = filename;
            this.models = models;
            framesPerSecond = models.Settings.MovieFps;
            firstFrame = 0;
            lastFrame = MaxFrameIndex;
            selectedPreset = AvailablePresets.Find(item => item.Cargo == models.Settings.MoviePreset);
        }

        public List<ListItemViewModel<FFMpeg.Preset>> AvailablePresets { get; } =
            new List<ListItemViewModel<FFMpeg.Preset>>
            {
                new ListItemViewModel<FFMpeg.Preset> { Cargo = FFMpeg.Preset.veryslow, Name = "very slow" },
                new ListItemViewModel<FFMpeg.Preset>{Cargo = FFMpeg.Preset.slower, Name = "slower" },
                new ListItemViewModel<FFMpeg.Preset>{Cargo = FFMpeg.Preset.slow, Name = "slow" },
                new ListItemViewModel<FFMpeg.Preset>{Cargo = FFMpeg.Preset.medium, Name = "medium" },
                new ListItemViewModel<FFMpeg.Preset>{Cargo = FFMpeg.Preset.fast, Name = "fast" },
                new ListItemViewModel<FFMpeg.Preset>{Cargo = FFMpeg.Preset.faster, Name = "faster" },
                new ListItemViewModel<FFMpeg.Preset>{Cargo = FFMpeg.Preset.superfast, Name = "super fast" },
                new ListItemViewModel<FFMpeg.Preset>{Cargo = FFMpeg.Preset.ultrafast, Name = "ultra fast" },
            };

        private ListItemViewModel<FFMpeg.Preset> selectedPreset;

        public ListItemViewModel<FFMpeg.Preset> SelectedPreset
        {
            get => selectedPreset;
            set
            {
                if (value == null || ReferenceEquals(value, selectedPreset)) return;
                selectedPreset = value;
                models.Settings.MoviePreset = selectedPreset.Cargo;
                OnPropertyChanged(nameof(SelectedPreset));
            }
        }

        public string Filename { get; set; }

        private int firstFrame;

        public int FirstFrame
        {
            get => firstFrame;
            set
            {
                if (value == firstFrame) return;
                firstFrame = value;
                OnPropertyChanged(nameof(FirstFrame));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private int lastFrame;

        public int LastFrame
        {
            get => lastFrame;
            set
            {
                if(value == lastFrame) return;
                lastFrame = value;
                OnPropertyChanged(nameof(LastFrame));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public int MaxFrameIndex => models.Images.NumLayers - 1;

        private int framesPerSecond;

        public int FramesPerSecond
        {
            get => framesPerSecond;
            set
            {
                if(value == framesPerSecond) return;
                framesPerSecond = value;
                OnPropertyChanged(nameof(FramesPerSecond));
            }
        }

        public bool IsValid => firstFrame <= lastFrame; // the other properties are guaranteed by the numeric box min/max values

        public FFMpeg.MovieExportConfig GetConfig()
        {
            return new FFMpeg.MovieExportConfig
            {
                Filename = Filename,
                FirstFrame = FirstFrame,
                FrameCount = LastFrame - FirstFrame + 1,
                Multiplier = 1.0f,
                FramesPerSecond = FramesPerSecond,
                Preset = SelectedPreset.Cargo
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
