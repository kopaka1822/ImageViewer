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

        public enum Statistics
        {
            Luminance = DefaultStatistics.Types.Luminance,
            Average = DefaultStatistics.Types.Average,
            Luma = DefaultStatistics.Types.Luma,
            Lightness = DefaultStatistics.Types.Lightness,
            Alpha = DefaultStatistics.Types.Alpha,
            SSIM
        }

        public SettingsModel()
        {
            // required if assembly version changes
            if (Properties.Settings.Default.UpdateSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
                Save();
            }

            Properties.Settings.Default.PropertyChanged += DefaultOnPropertyChanged;
        }

        private void DefaultOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Properties.Settings.Default.TexelDisplayMode):
                    OnPropertyChanged(nameof(TexelDisplay));
                    break;
                case nameof(Properties.Settings.Default.TexelDecimalCount):
                    OnPropertyChanged(nameof(TexelDecimalPlaces));
                    break;
                case nameof(Properties.Settings.Default.FlipYAxis):
                    OnPropertyChanged(nameof(FlipYAxis));
                    break;
                case nameof(Properties.Settings.Default.AlphaBackground):
                    OnPropertyChanged(nameof(AlphaBackground));
                    break;
            }
        }

        public int WindowWidth
        {
            get => Properties.Settings.Default.WindowWidth;
            set
            {
                if(value > 0)
                    Properties.Settings.Default.WindowWidth = value;
            } 
        }

        public int WindowHeight
        {
            get => Properties.Settings.Default.WindowHeight;
            set
            {
                if (value > 0)
                    Properties.Settings.Default.WindowHeight = value;
            }
        }

        public bool IsMaximized
        {
            get => Properties.Settings.Default.IsMaximized;
            set => Properties.Settings.Default.IsMaximized = value;
        }

        public ThemeDictionary.Themes Theme
        {
            get => (ThemeDictionary.Themes)Properties.Settings.Default.Theme;
            set
            {
                if (value < 0 || value >= ThemeDictionary.Themes.Size) return;
                if (value == Theme) return;
                Properties.Settings.Default.Theme = (int)value;
            }
        }

        public string ImagePath
        {
            get => Properties.Settings.Default.ImagePath ?? "";
            set => Properties.Settings.Default.ImagePath = value;
        }

        public string FilterPath
        {
            get => Properties.Settings.Default.FilterPath ?? "";
            set => Properties.Settings.Default.FilterPath = value;
        }
        public Statistics StatisticsChannel
        {
            get => (Statistics)Properties.Settings.Default.StatisticsChannel;
            set => Properties.Settings.Default.StatisticsChannel = (int) value;
        }

        /// <summary>
        /// json export quality
        /// </summary>
        public int LastQuality
        {
            get => Properties.Settings.Default.LastQuality;
            set => Properties.Settings.Default.LastQuality = value;
        }

        public TexelDisplayMode TexelDisplay
        {
            get => (TexelDisplayMode)Properties.Settings.Default.TexelDisplayMode;
            set => Properties.Settings.Default.TexelDisplayMode = (int) value;
        }

        public int MinTexelDecimalPlaces { get; } = 2;
        public int MaxTexelDecimalPlaces { get; } = 10;

        public int TexelDecimalPlaces
        {
            get => Properties.Settings.Default.TexelDecimalCount;
            set
            {
                var clamp = Utility.Clamp(value, MinTexelDecimalPlaces, MaxTexelDecimalPlaces);
                Properties.Settings.Default.TexelDecimalCount = clamp;
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
            get => (AlphaType)Properties.Settings.Default.AlphaBackground;
            set => Properties.Settings.Default.AlphaBackground = (int)value;
        }

        public bool FlipYAxis
        {
            get => Properties.Settings.Default.FlipYAxis;
            set => Properties.Settings.Default.FlipYAxis = value;
        }

        public Color NaNColor
        {
            get => new Color(Properties.Settings.Default.NanRed, Properties.Settings.Default.NanGreen, Properties.Settings.Default.NanBlue);
            set
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (Properties.Settings.Default.NanRed == value.Red &&
                    Properties.Settings.Default.NanGreen == value.Green &&
                    Properties.Settings.Default.NanBlue == value.Blue) return;

                Properties.Settings.Default.NanRed = value.Red;
                Properties.Settings.Default.NanGreen = value.Green;
                Properties.Settings.Default.NanBlue = value.Blue;
                OnPropertyChanged(nameof(NaNColor));
            }
        }

        public Color ZoomBoxColor
        {
            get => new Color(Properties.Settings.Default.ZoomBoxRed, Properties.Settings.Default.ZoomBoxGreen, Properties.Settings.Default.ZoomBoxBlue);
            set
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (Properties.Settings.Default.ZoomBoxRed == value.Red &&
                    Properties.Settings.Default.ZoomBoxGreen == value.Green &&
                    Properties.Settings.Default.ZoomBoxBlue == value.Blue) return;

                Properties.Settings.Default.ZoomBoxRed = value.Red;
                Properties.Settings.Default.ZoomBoxGreen = value.Green;
                Properties.Settings.Default.ZoomBoxBlue = value.Blue;
                OnPropertyChanged(nameof(ZoomBoxColor));
                OnPropertyChanged(nameof(ArrowColor));
            }
        }

        public Color ArrowColor // for now shared with zoom box
        {
            get => ZoomBoxColor;
            set => ZoomBoxColor = value;
        }

        public int ZoomBoxBorder
        {
            get => Properties.Settings.Default.ZoomBoxBorder;
            set
            {
                if(Properties.Settings.Default.ZoomBoxBorder == value) return;
                Properties.Settings.Default.ZoomBoxBorder = value;
                OnPropertyChanged(nameof(ZoomBoxBorder));
                OnPropertyChanged(nameof(ArrowWidth));
            }
        }

        public int ArrowWidth // for now shared with zoom box
        {
            get => ZoomBoxBorder;
            set => ZoomBoxBorder = value;
        }

        public bool ExportZoomBoxBorder
        {
            get => Properties.Settings.Default.ExportZoomBoxBorder;
            set
            {
                if(Properties.Settings.Default.ExportZoomBoxBorder == value) return;
                Properties.Settings.Default.ExportZoomBoxBorder = value;
                OnPropertyChanged(nameof(ExportZoomBoxBorder));
            }
        }

        public int ExportZoomBoxScale
        {
            get => Properties.Settings.Default.ExportZoomBoxScale;
            set
            {
                if (Properties.Settings.Default.ExportZoomBoxScale == value) return;
                Properties.Settings.Default.ExportZoomBoxScale = value;
                OnPropertyChanged(nameof(ExportZoomBoxScale));
            }
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
