using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace OpenTKImageViewer.glhelper
{
    /// <summary>
    /// a texture that is cached on cpu side
    /// </summary>
    public class CpuTexture
    {
        internal struct LevelData
        {
            public float[] Pixels;
            public int Width;
            public int Height;
        }

        private readonly LevelData[] pixels;
        private readonly int numFaces;

        public CpuTexture(int numLevel, int numFaces)
        {
            this.numFaces = numFaces;
            pixels = new LevelData[numLevel];
        }

        /// <summary>
        /// sets data for a level of the texture
        /// </summary>
        /// <param name="data">data with all faces (rgba)</param>
        /// <param name="level">texture level</param>
        /// <param name="width">width of the level</param>
        /// <param name="height">height of the level</param>
        public void SetLevel(float[] data, int level, int width, int height)
        {
            pixels[level] = new LevelData{Pixels = data, Width = width, Height = height};
            Debug.Assert(data.Length == width * height * 4 * numFaces);
        }

        /// <summary>
        /// clamps the x and y values corresponding to level dimension and returns the pixel.
        /// returns Vector4(0) if face or level are invalid
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="face"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public Vector4 GetPixel(int x, int y, int face, int level)
        {
            if(level >= pixels.Length || face >= numFaces)
                return new Vector4(0.0f);
            x = Math.Min(Math.Max(x, 0), pixels[level].Width - 1);
            y = Math.Min(Math.Max(y, 0), pixels[level].Height - 1);
            return GetPixelRaw(x, y, face, level);
        }

        private Vector4 GetPixelRaw(int x, int y, int face, int level)
        {
            var ldata = pixels[level];
            // pictures are upside down
            y = ldata.Height - y - 1;
            var faceSize = ldata.Width * ldata.Height * 4;
            int idx = faceSize * face + (y * ldata.Width + x) * 4;
            return new Vector4(ldata.Pixels[idx], ldata.Pixels[idx + 1], ldata.Pixels[idx + 2], ldata.Pixels[idx + 3]);
        }
    }
}
