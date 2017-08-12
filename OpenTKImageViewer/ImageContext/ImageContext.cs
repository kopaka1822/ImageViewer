using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.Tonemapping;

namespace OpenTKImageViewer.ImageContext
{
    public delegate void ChangedLayerHandler(object sender, EventArgs e);

    public delegate void ChangedMipmapHanlder(object sender, EventArgs e);

    public delegate void ChangedImagesHandler(object sender, EventArgs e);

    public delegate void ChangedFilteringHandler(object sender, EventArgs e);

    public delegate void ChangedGrayscaleHandler(object sender, EventArgs e);

    public class ImageContext
    {
        #region Structures and Enums

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

        #endregion

        #region Private Member

        private readonly List<ImageData> images = new List<ImageData>();
        private uint activeMipmap = 0;
        private uint activeLayer = 0;
        private TextureArray2D combinedImage1;
        private readonly ImageCombineShader imageCombineShader1;
        private bool linearInterpolation = false;
        private GrayscaleMode grayscale = GrayscaleMode.Disabled;

        #endregion

        #region Public Properties

        public bool LinearInterpolation
        {
            get => linearInterpolation;
            set
            {
                if (value == linearInterpolation) return;
                linearInterpolation = value;
                OnChangedFiltering();
            }
        }

        public GrayscaleMode Grayscale
        {
            get => grayscale;
            set
            {
                if (value == grayscale) return;
                grayscale = value;
                OnChangedGrayscale();
            }
        }
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
                if (value != activeLayer && value < GetNumLayers())
                {
                    activeLayer = value;
                    OnChangedLayer();
                }
            }
        }

        public ImageFormula ImageFormula1 { get; } = new ImageFormula();
        public TonemapperManager Tonemapper { get; } = new TonemapperManager();

        #endregion

        #region Public Getter

        public int GetNumImages()
        {
            return images.Count;
        }

        public int GetNumMipmaps()
        {
            if (images.Count == 0)
                return 0;
            return images[0].image.GetNumMipmaps();
        }

        public int GetNumLayers()
        {
            if (images.Count == 0)
                return 0;
            return images[0].image.Layers.Count;
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

        public string GetFilename(int image)
        {
            Debug.Assert((uint)(image) < images.Count);
            return images[image].image.Filename;
        }

        public bool HasOnlyGrayscale()
        {
            foreach (var imageData in images)
            {
                if (!imageData.image.IsGrayscale())
                    return false;
            }
            return true;
        }

        public byte[] GetCurrentImageData(int level, int layer, PixelFormat format, PixelType type, out int width,
            out int height)
        {
            width = GetWidth(level);
            height = GetHeight(level);
            if (combinedImage1 == null)
                return null;

            return combinedImage1.GetData(level, layer, format, type, out width, out height);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// trues to add the image. Throws Exception if image could not be added
        /// </summary>
        /// <param name="image"></param>
        public void AddImage(ImageLoader.Image image)
        {
            // only add if layout is consistent
            if (images.Count > 0)
            {
                var i = images[0].image;
                if (image.Layers.Count != i.Layers.Count)
                    throw new Exception($"Inconsistent amount of layers. Expected {i.Layers.Count} got {image.Layers.Count}");

                if (image.GetNumMipmaps() != i.GetNumMipmaps())
                    throw new Exception($"Inconsistent amount of mipmaps. Expected {i.GetNumMipmaps()} got {image.GetNumMipmaps()}");

                // test mipmaps
                for (int level = 0; level < image.GetNumMipmaps(); ++level)
                {
                    if (image.GetWidth(level) != i.GetWidth(level) || image.GetHeight(level) != i.GetHeight(level))
                        throw new Exception($"Inconsistent mipmaps dimension. Expected {i.GetWidth(level)}x{i.GetHeight(level)}" +
                                            $" got {image.GetWidth(level)}x{image.GetHeight(level)}");
                }
            }

            images.Add(new ImageData(image));
            OnChangedImages();
            if(HasOnlyGrayscale())
                Grayscale = GrayscaleMode.Red;
        }

        public void BindFinalTextureAs2DSamplerArray(int slot)
        {
            combinedImage1?.Bind(slot, LinearInterpolation);
        }

        public void BindFinalTextureAsCubeMap(int slot)
        {
            combinedImage1?.BindAsCubemap(slot, LinearInterpolation);
        }

        /// <summary>
        /// should be called before drawing the final image in order to update its contents if required
        /// </summary>
        public void Update()
        {
            if (images.Count == 0)
                return;

            // create gpu textures for newly added images
            foreach (var imageData in images)
            {
                if (imageData.TextureArray2D == null)
                    imageData.TextureArray2D = new TextureArray2D(imageData.image);
            }

            // create destination images
            if (combinedImage1 == null)
            {
                // create image
                combinedImage1 = new TextureArray2D(GetNumLayers(), GetNumMipmaps(),
                    SizedInternalFormat.Rgba32f, GetWidth(0), GetHeight(0));
            }

            // update image combine shader
            if(imageCombineShader1.Update())
                RecomputeCombinedImage(combinedImage1);

        }

        #endregion

        #region Events

        public event ChangedLayerHandler ChangedLayer;
        public event ChangedMipmapHanlder ChangedMipmap;
        public event ChangedImagesHandler ChangedImages;
        public event ChangedFilteringHandler ChangedFiltering;
        public event ChangedGrayscaleHandler ChangedGrayscale;

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

        protected virtual void OnChangedFiltering()
        {
            ChangedFiltering?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnChangedGrayscale()
        {
            ChangedGrayscale?.Invoke(this, EventArgs.Empty);
        }

        #endregion


        public ImageContext(List<ImageLoader.Image> images)
        {
            imageCombineShader1 = new ImageCombineShader(this, ImageFormula1);
            if (images != null)
            {
                foreach (var image in images)
                {
                    AddImage(image);
                }
            }
        }

        private void RecomputeCombinedImage(TextureArray2D target)
        {
            if (images.Count == 0)
                return;

            for (int layer = 0; layer < GetNumLayers(); ++layer)
            {
                for (int level = 0; level < GetNumMipmaps(); ++level)
                {
                    for (int image = 0; image < GetNumImages(); ++image)
                    {
                        images[image].TextureArray2D.Bind(imageCombineShader1.GetSourceImageBinding(image), false);
                    }
                    target.BindAsImage(imageCombineShader1.GetDestinationImageBinding(),
                        level, layer, TextureAccess.WriteOnly);
                    
                    imageCombineShader1.Run(layer, level, GetWidth(level), GetHeight(level));
                }
            }
        }
    }
}
