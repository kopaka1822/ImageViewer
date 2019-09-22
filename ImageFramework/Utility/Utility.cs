using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageFramework.Utility
{
    public static class Utility
    {
        /// <summary>
        /// transforms coordinates from [-1, 1] to [0, imagesize - 1].
        /// clamps values if coordinates are not within range
        /// </summary>
        /// <param name="coord">[-1, 1]</param>
        /// <param name="imageWidth">width in pixels</param>
        /// <param name="imageHeight">height in pixel</param>
        /// <returns></returns>
        /*public static Point CanonicalToTexelCoordinates(Vector2 coord, int imageWidth, int imageHeight)
        {
            // trans mouse is betweem [-1,1] in texture coordinates => to [0,1]
            coord.X += 1.0f;
            coord.X /= 2.0f;

            coord.Y += 1.0f;
            coord.Y /= 2.0f;

            // clamp value
            coord.X = Math.Min(0.9999f, Math.Max(0.0f, coord.X));
            coord.Y = Math.Min(0.9999f, Math.Max(0.0f, coord.Y));

            // scale with mipmap level
            coord.X *= (float)imageWidth;
            coord.Y *= (float)imageHeight;

            return new Point((int)(coord.X), (int)(coord.Y));
        }*/

        /// <summary>
        /// opens the file dialog for images
        /// </summary>
        /// <returns>string with filenames or null if aborted</returns>
        /*public static string[] ShowImportImageDialog(Window parent)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = Properties.Settings.Default.ImagePath
            };

            if (ofd.ShowDialog(parent) != true) return null;

            // set new image path in settings
            Properties.Settings.Default.ImagePath = System.IO.Path.GetDirectoryName(ofd.FileName);
            return ofd.FileNames;
        }*/

        /// <summary>
        /// calculates a/b and adds one if the remainder (a%b) is not zero
        /// </summary>
        /// <param name="a">nominator</param>
        /// <param name="b">denominator</param>
        /// <returns></returns>
        public static int DivideRoundUp(int a, int b)
        {
            Debug.Assert(b > 0);
            Debug.Assert(a >= 0);
            return (a + b - 1) / b;
        }
    }
}
