using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTKImageViewer.glhelper;

namespace OpenTKImageViewer.ImageContext
{
    public class ImageContext
    {
        private class ImageData
        {
            public ImageLoader.Image image;
            public TextureArray2D TextureArray2D;

            public ImageData(ImageLoader.Image image)
            {
                this.image = image;
            }
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

        public void Update()
        {
            // create gpu textures for newly added images
            foreach (var imageData in images)
            {
                if(imageData.TextureArray2D == null)
                    imageData.TextureArray2D = new TextureArray2D(imageData.image);
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

        public void BindFinalTextureAs2DSamplerArray(int slot)
        {
            // TODO replace with correct code
            if (images.Count > 0)
            {
                images[0].TextureArray2D.Bind(slot);
            }
        }

        public int GetWidth(int mipmap)
        {
            Debug.Assert(images.Count != 0);
            return images[0].image.GetWidth(mipmap);
        }

        public int GetHeight(int mipmap)
        {
            Debug.Assert(images.Count != 0);
            return images[0].image.GetHeight(mipmap);
        }
    }
}
