using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;

namespace OpenTKImageViewer.ImageContext
{
    public delegate void ChangedLayerHandler(object sender, EventArgs e);

    public delegate void ChangedMipmapHanlder(object sender, EventArgs e);

    public delegate void ChangedImagesHandler(object sender, EventArgs e);

    public delegate void ChangedImagesCombineFormulaHandler(object sender, EventArgs e);

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
        private string imageCombineFormula = "GetTexture0()";
        private TextureArray2D combinedImages;
        private ImageCombineShader imageCombineShader;

        #endregion

        #region Public Properties

        public bool LinearInterpolation { get; set; } = false;
        public GrayscaleMode Grayscale { get; set; } = GrayscaleMode.Disabled;
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

        public string ImageCombineFormula
        {
            get { return imageCombineFormula; }
            set
            {
                if (imageCombineFormula != value)
                {
                    imageCombineFormula = value;
                    OnChangedImageCombineFormula();
                }
            }
        }

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
        }

        public void BindFinalTextureAs2DSamplerArray(int slot)
        {
            // TODO replace with correct code
            /*if (images.Count > 0)
            {
                images[0].TextureArray2D.Bind(slot);
            }*/
            combinedImages?.Bind(slot);
        }

        public void BindFinalTextureAsCubeMap(int slot)
        {
            // TODO replace with correct code
            if (images.Count > 0)
            {
                //images[0].TextureArray2D.BindAs(slot, TextureTarget.TextureCubeMap);
                //images[0].TextureArray2D.Bind(slot);
                images[0].TextureArray2D.BindAsCubemap(slot);
            }
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

            if (combinedImages == null)
            {
                // create image
                combinedImages = new TextureArray2D(GetNumLayers(), GetNumMipmaps(),
                    SizedInternalFormat.Rgba32f, GetWidth(0), GetHeight(0));
            }

            imageCombineShader.Update();

            RecomputeCombinedImage();
        }

        #endregion

        #region Events

        public event ChangedLayerHandler ChangedLayer;
        public event ChangedMipmapHanlder ChangedMipmap;
        public event ChangedImagesHandler ChangedImages;
        public event ChangedImagesCombineFormulaHandler ChangedImageCombineFormula;

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

        protected virtual void OnChangedImageCombineFormula()
        {
            ChangedImageCombineFormula?.Invoke(this, EventArgs.Empty);
        }

        #endregion
        

        public ImageContext(List<ImageLoader.Image> images)
        {
            imageCombineShader = new ImageCombineShader(this);
            if (images != null)
            {
                foreach (var image in images)
                {
                    AddImage(image);
                }
            }
        }

        
        

        private void RecomputeCombinedImage()
        {
            if (images.Count == 0)
                return;

            for (int layer = 0; layer < GetNumLayers(); ++layer)
            {
                for (int level = 0; level < GetNumMipmaps(); ++level)
                {
                    for (int image = 0; image < GetNumImages(); ++image)
                    {
                        images[image].TextureArray2D.Bind(imageCombineShader.GetSourceImageBinding(image));
                    }
                    combinedImages.BindAsImage(imageCombineShader.GetDestinationImageBinding(),
                        level, layer, TextureAccess.WriteOnly);
                    
                    imageCombineShader.Run(layer, level, GetWidth(level), GetHeight(level));
                }
            }
        }

    }
}
