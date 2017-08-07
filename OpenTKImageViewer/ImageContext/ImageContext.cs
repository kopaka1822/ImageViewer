using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            public ImageData(ImageLoader.Image image)
            {
                this.image = image;
            }

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

        public ImageContext(List<ImageLoader.Image> images)
        {
            if (images != null)
            {
                foreach (var image in images)
                {
                    AddImage(image);
                }
            }
        }

        public int GetNumImages()
        {
            return images.Count;
        }

        public void AddImage(ImageLoader.Image image)
        {
            images.Add(new ImageData(image));
        }
    }
}
