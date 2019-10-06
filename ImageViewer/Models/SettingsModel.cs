using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageViewer.Properties;
using ImageViewer.Views.Theme;

namespace ImageViewer.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public int WindowWidth { get; set; } = 800;

        public int WindowHeight { get; set; } = 600;
        public bool IsMaximized { get; set; } = false;

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
