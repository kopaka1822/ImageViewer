using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model.Export;
using ImageFramework.Model.Shader;
using ImageFramework.Model.Statistics;
using ImageFramework.Utility;
using ImageViewer.Properties;
using ImageViewer.ViewModels;
using ImageViewer.Views.Theme;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        private const int MAX_RECENT_ENTRIES = 15;

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
                try
                {
                    Properties.Settings.Default.Upgrade();
                }
                catch (Exception)
                {
                    // ignored
                }

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
                case nameof(Properties.Settings.Default.MovieFps):
                    OnPropertyChanged(nameof(MovieFps));
                    break;
                case nameof(Properties.Settings.Default.MovieRepeat):
                    OnPropertyChanged(nameof(MovieRepeat));
                    break;
                case nameof(Properties.Settings.Default.MoviePreset):
                    OnPropertyChanged(nameof(MoviePreset));
                    break;
                case nameof(Properties.Settings.Default.RecentFiles):
                    OnPropertyChanged(nameof(RecentFiles));
                    break;
                case nameof(Properties.Settings.Default.HdrMode):
                    OnPropertyChanged(nameof(HdrMode));
                    break;
                case nameof(Properties.Settings.Default.CachePrecision):
                    OnPropertyChanged(nameof(CachePrecision));
                    OnPropertyChanged(nameof(CacheFormat));
                    break;
                case nameof(Properties.Settings.Default.ImageNameOverlay):
                    OnPropertyChanged(nameof(ImageNameOverlay));
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
            get
            {
                // check if directory exists
                if (System.IO.Directory.Exists(Properties.Settings.Default.ImagePath))
                    return Properties.Settings.Default.ImagePath;
                return "";
            }
            set => Properties.Settings.Default.ImagePath = value;
        }

        public string FilterPath
        {
            get
            {
                // check if directory exists
                if (System.IO.Directory.Exists(Properties.Settings.Default.FilterPath))
                    return Properties.Settings.Default.FilterPath;
                return "";
            }
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

        public float MovieFps
        {
            get => Properties.Settings.Default.MovieFps;
            set
            {
                var clamp = Utility.Clamp(value, 1, 300);
                Properties.Settings.Default.MovieFps = clamp;
            }
        }

        public enum MovieRepeatMode
        {
            NoRepeat, // play once
            Repeat, // repeat after end
            Mirror // mirror after end
        }
        public MovieRepeatMode MovieRepeat
        {
            get => (MovieRepeatMode)Properties.Settings.Default.MovieRepeat;
            set => Properties.Settings.Default.MovieRepeat = (int)value;
        }

        public FFMpeg.Preset MoviePreset
        {
            get => (FFMpeg.Preset)Properties.Settings.Default.MoviePreset;
            set => Properties.Settings.Default.MoviePreset = (int)value;
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

        public bool HdrMode
        {
            set => Properties.Settings.Default.HdrMode = value;
            get => Properties.Settings.Default.HdrMode && Device.Get().IsHDR;
        }

        public bool ImageNameOverlay
        {
            get => Properties.Settings.Default.ImageNameOverlay;
            set => Properties.Settings.Default.ImageNameOverlay = value;
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

        public System.Collections.Specialized.StringCollection RecentFiles
        {
            get
            {
                var res = Properties.Settings.Default.RecentFiles;
                if (res == null) res = Properties.Settings.Default.RecentFiles = new System.Collections.Specialized.StringCollection();
                return res;
            }
        }

        public void AddRecentFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) return;
            
            var list = RecentFiles;
            // remove filename if it already exists
            list.Remove(filename);
            list.Insert(0, filename);

            // only keep MAX_RECENT_ENTRIES
            while (list.Count > MAX_RECENT_ENTRIES)
                list.RemoveAt(list.Count - 1);

            OnPropertyChanged(nameof(RecentFiles));
        }

        public enum CachePrecisionType
        {
            Float, // = default
            Byte,
        }

        // cache precision is saved for the active instance and resets to float on default to prevent accidental precision loss
        private CachePrecisionType cachePrecision = CachePrecisionType.Float;
        public CachePrecisionType CachePrecision
        {
            //get => (CachePrecisionType) Properties.Settings.Default.CachePrecision;
            //set => Properties.Settings.Default.CachePrecision = (int)value;
            get => cachePrecision;
            set
            {
                if (cachePrecision == value) return;
                cachePrecision = value;
                OnPropertyChanged(nameof(CachePrecision));
                OnPropertyChanged(nameof(CacheFormat));
            }
        }

        public Format CacheFormat
        {
            get
            {
                switch (CachePrecision)
                {
                    case CachePrecisionType.Byte:
                        return Format.R8G8B8A8_UNorm;
                    case CachePrecisionType.Float:
                        return Format.R32G32B32A32_Float;
                }
                Debug.Assert(false);
                return Format.Unknown;
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
