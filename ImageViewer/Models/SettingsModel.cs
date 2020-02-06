using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model.Shader;
using ImageFramework.Model.Statistics;
using ImageFramework.Utility;
using ImageViewer.Properties;
using ImageViewer.ViewModels;
using ImageViewer.Views.Theme;

namespace ImageViewer.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public enum TexelDisplayMode
        {
            LinearDecimal,
            LinearFloat,
            SrgbDecimal,
            SrgbByte
        }

        public SettingsModel()
        {
            // required if assembly version changes
            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                Save();
            }
            
            Settings.Default.PropertyChanged += DefaultOnPropertyChanged;
        }

        private void DefaultOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.Default.TexelDisplayMode):
                    OnPropertyChanged(nameof(TexelDisplay));
                    break;
                case nameof(Settings.Default.TexelDecimalCount):
                    OnPropertyChanged(nameof(TexelDecimalPlaces));
                    break;
                case nameof(Settings.Default.FlipYAxis):
                    OnPropertyChanged(nameof(FlipYAxis));
                    break;
                case nameof(Settings.Default.AlphaBackground):
                    OnPropertyChanged(nameof(AlphaBackground));
                    break;
            }
        }

        public int WindowWidth
        {
            get => Settings.Default.WindowWidth;
            set
            {
                if(value > 0)
                    Settings.Default.WindowWidth = value;
            } 
        }

        public int WindowHeight
        {
            get => Settings.Default.WindowHeight;
            set
            {
                if (value > 0)
                    Settings.Default.WindowHeight = value;
            }
        }

        public bool IsMaximized
        {
            get => Settings.Default.IsMaximized;
            set => Settings.Default.IsMaximized = value;
        }

        public ThemeDictionary.Themes Theme
        {
            get => (ThemeDictionary.Themes)Settings.Default.Theme;
            set
            {
                if (value < 0 || value >= ThemeDictionary.Themes.Size) return;
                if (value == Theme) return;
                Settings.Default.Theme = (int)value;
            }
        }

        public string ImagePath
        {
            get => Settings.Default.ImagePath ?? "";
            set => Settings.Default.ImagePath = value;
        }

        public string FilterPath
        {
            get => Settings.Default.FilterPath ?? "";
            set => Settings.Default.FilterPath = value;
        }
        public DefaultStatistics.Types StatisticsChannel
        {
            get => (DefaultStatistics.Types) Settings.Default.StatisticsChannel;
            set => Settings.Default.StatisticsChannel = (int) value;
        }

        /// <summary>
        /// json export quality
        /// </summary>
        public int LastQuality
        {
            get => Settings.Default.LastQuality;
            set => Settings.Default.LastQuality = value;
        }

        public TexelDisplayMode TexelDisplay
        {
            get => (TexelDisplayMode)Settings.Default.TexelDisplayMode;
            set => Settings.Default.TexelDisplayMode = (int) value;
        }

        public int MinTexelDecimalPlaces { get; } = 2;
        public int MaxTexelDecimalPlaces { get; } = 10;

        public int TexelDecimalPlaces
        {
            get => Settings.Default.TexelDecimalCount;
            set
            {
                var clamp = Utility.Clamp(value, MinTexelDecimalPlaces, MaxTexelDecimalPlaces);
                Settings.Default.TexelDecimalCount = clamp;
            }
        }

        public enum AlphaType
        {
            Black,
            White,
            Checkers,
            Theme
        }

        public AlphaType AlphaBackground
        {
            get => (AlphaType) Settings.Default.AlphaBackground;
            set => Settings.Default.AlphaBackground = (int)value;
        }

        public bool FlipYAxis
        {
            get => Settings.Default.FlipYAxis;
            set => Settings.Default.FlipYAxis = value;
        }

        public Color NaNColor
        {
            get => new Color(Settings.Default.NanRed, Settings.Default.NanGreen, Settings.Default.NanBlue);
            set
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (Settings.Default.NanRed == value.Red && 
                    Settings.Default.NanGreen == value.Green && 
                    Settings.Default.NanBlue == value.Blue) return;

                Settings.Default.NanRed = value.Red;
                Settings.Default.NanGreen = value.Green;
                Settings.Default.NanBlue = value.Blue;
                OnPropertyChanged(nameof(NaNColor));
            }
        }

        public Color ZoomBoxColor
        {
            get => new Color(Settings.Default.ZoomBoxRed, Settings.Default.ZoomBoxGreen, Settings.Default.ZoomBoxBlue);
            set
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (Settings.Default.ZoomBoxRed == value.Red &&
                    Settings.Default.ZoomBoxGreen == value.Green &&
                    Settings.Default.ZoomBoxBlue == value.Blue) return;

                Settings.Default.ZoomBoxRed = value.Red;
                Settings.Default.ZoomBoxGreen = value.Green;
                Settings.Default.ZoomBoxBlue = value.Blue;
                OnPropertyChanged(nameof(ZoomBoxColor));
            }
        }

        public int ZoomBoxBorder
        {
            get => Settings.Default.ZoomBoxBorder;
            set
            {
                if(Settings.Default.ZoomBoxBorder == value) return;
                Settings.Default.ZoomBoxBorder = value;
                OnPropertyChanged(nameof(ZoomBoxBorder));
            }
        }

        public void Save()
        {
            Settings.Default.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
