using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace TextureViewer.Utility
{
    static class Utility
    {
        /// <summary>
        /// transforms coordinates from [-1, 1] to [0, imagesize - 1].
        /// clamps values if coordinates are not within range
        /// </summary>
        /// <param name="coord">[-1, 1]</param>
        /// <param name="imageWidth">width in pixels</param>
        /// <param name="imageHeight">height in pixel</param>
        /// <returns></returns>
        public static Point CanonicalToTexelCoordinates(Vector2 coord, int imageWidth, int imageHeight)
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
        }
    }
}
