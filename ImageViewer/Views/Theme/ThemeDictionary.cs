using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            Size
        }

        private Themes curTheme;

        public ThemeDictionary()
        {
            var themeIdx = Settings.Default.Theme;
            if (themeIdx < 0 || themeIdx >= (int) (Themes.Size))
                themeIdx = 0;

            curTheme = (Themes) themeIdx;
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

        private void UpdateSource(Uri theme)
        {
            base.Source = theme;
        }
    }
}
