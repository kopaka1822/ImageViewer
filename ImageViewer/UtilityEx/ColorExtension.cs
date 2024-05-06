using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Color = ImageFramework.Utility.Color;

namespace ImageViewer.UtilityEx
{
    public static class ColorExtension
    {
        public static System.Windows.Media.Color ToMediaColor(this Color c)
        {
            return System.Windows.Media.Color.FromScRgb(c.Alpha, c.Red, c.Green, c.Blue);
        }

        public static System.Windows.Media.SolidColorBrush ToBrush(this Color c)
        {
            return new SolidColorBrush(c.ToMediaColor());
        }

        public static Color ToFramework(this System.Windows.Media.SolidColorBrush c)
        {
            return new Color(c.Color.ScR, c.Color.ScG, c.Color.ScB, c.Color.ScA);
        }
    }
}
