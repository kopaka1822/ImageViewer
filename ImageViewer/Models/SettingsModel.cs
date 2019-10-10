using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model.Shader;
using ImageViewer.Properties;
using ImageViewer.ViewModels;
using ImageViewer.Views.Theme;

namespace ImageViewer.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
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
        public DefaultStatistics.Values StatisticsChannel
        {
            get => (DefaultStatistics.Values) Settings.Default.StatisticsChannel;
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
