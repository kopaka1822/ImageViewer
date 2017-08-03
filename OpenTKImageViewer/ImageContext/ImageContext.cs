using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.ImageContext
{
    public class ImageContext
    {
        private class ImageData
        {
            public ImageLoader.Image image;
            // TODO add opengl texture
        }

        public enum GrayscaleMode
        {
            Disabled,
            Red,
            Green,
            Blue,
            Alpha
        }

        private readonly List<ImageData> images = new List<ImageData>();
        private uint activeMipmap = 0;
        private uint activeLayer = 0;
        public bool LinearInterpolation { get; set; } = false;
        public GrayscaleMode Grayscale { get; set; } = GrayscaleMode.Disabled;

        
    }
}
