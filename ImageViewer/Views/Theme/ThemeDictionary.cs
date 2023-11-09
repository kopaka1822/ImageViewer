using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageViewer.Models;
using ImageViewer.Properties;

namespace ImageViewer.Views.Theme
{
    public class ThemeDictionary : ResourceDictionary
    {
        public enum Themes
        {
            Default,
            White,
            Dark,
            Black,
            Size
        }

        private Themes curTheme;

        public ThemeDictionary()
        {
            var settings = new SettingsModel();
            if ((uint) settings.Theme >= (uint)Themes.Size)
            {
                settings.Theme = Themes.Default;
            }
            curTheme = settings.Theme;
        }

        public Uri DefaultSource
        {
            set
            {
                if(curTheme == Themes.Default)
                    UpdateSource(value);
            }
        }

        public Uri WhiteSource
        {
            set
            {
                if(curTheme == Themes.White)
                    UpdateSource(value);
            }
        }

        public Uri DarkSource
        {
            set
            {
                if(curTheme == Themes.Dark)
                    UpdateSource(value);
            }
        }

        public Uri BlackSource
        {
            set
            {
                if (curTheme == Themes.Black)
                    UpdateSource(value);
            }
        }

        private void UpdateSource(Uri theme)
        {
            base.Source = theme;
        }
    }
}
