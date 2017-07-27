using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.ImageView
{
    public delegate void ChangedLayerHandler(object sender, EventArgs e);

    public delegate void ChangedMipmapHanlder(object sender, EventArgs e);

    public delegate void ChangedImagesHandler(object sender, EventArgs e);

    public class Context
    {
        private readonly List<ImageLoaderWrapper.Image> images = new List<ImageLoaderWrapper.Image>();
        private uint activeMipmap = 0;
        private uint activeLayer = 0;

        public bool LinearInterpolation { get; set; } = true;

        public uint ActiveMipmap
        {
            get { return activeMipmap; }
            set
            {
                if (value != activeMipmap && value < GetNumMipmaps())
                {
                    activeMipmap = value;
                    OnChangedMipmap();
                }
            }
        }

        public uint ActiveLayer
        {
            get { return activeLayer; }
            set
            {
                if (value != activeLayer)
                {
                    activeLayer = value;
                    OnChangedLayer();
                }
            }
        }

        public void AddImage(ImageLoaderWrapper.Image image)
        {
            if (image == null)
                return;
            images.Add(image);
            OnChangedImages();
        }

        public List<ImageLoaderWrapper.Image> GetImages()
        {
            return images;
        }

        public int GetNumMipmaps()
        {
            if (images.Count == 0)
                return 0;
            return images[0].GetNumMipmaps();
        }

        public int GetNumLayers()
        {
            if (images.Count == 0)
                return 0;
            return images[0].Layers.Count;
        }

        public int GetNumImages()
        {
            return images.Count;
        }

        public int GetWidth(int mipmap)
        {
            Debug.Assert(images.Count != 0);
            return images[0].GetWidth(mipmap);
        }

        public int GetHeight(int mipmap)
        {
            Debug.Assert(images.Count != 0);
            return images[0].GetHeight(mipmap);
        }

        public event ChangedLayerHandler ChangedLayer;
        public event ChangedMipmapHanlder ChangedMipmap;
        public event ChangedImagesHandler ChangedImages;

        protected virtual void OnChangedLayer()
        {
            ChangedLayer?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnChangedMipmap()
        {
            ChangedMipmap?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnChangedImages()
        {
            ChangedImages?.Invoke(this, EventArgs.Empty);
        }

        public Context()
        {}

        public Context(ImageLoaderWrapper.Image image)
        {
            AddImage(image);
        }
    }
}
