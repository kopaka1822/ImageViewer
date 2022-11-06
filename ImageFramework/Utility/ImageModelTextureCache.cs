using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model;
using SharpDX.DXGI;

namespace ImageFramework.Utility
{
    /// <summary>
    /// texture cache that is synchronized with images model
    /// </summary>
    public class ImageModelTextureCache : ITextureCache
    {
        private readonly Stack<ITexture> textures = new Stack<ITexture>(2);
        private readonly ImagesModel images;
        private readonly Format format;
        private readonly bool createUav;
        private readonly bool createRtv;
        private TextureArray2D[] staging2D;

        public ImageModelTextureCache(ImagesModel images, Format format = Format.R32G32B32A32_Float, bool createUav = true, bool createRtv = true)
        {
            this.images = images;
            this.createUav = createUav;
            this.createRtv = createRtv;
            this.format = format;
            images.PropertyChanged += ImagesOnPropertyChanged;
        }

        public ITexture GetTexture()
        {
            if (textures.Count > 0) return textures.Pop();

            // make new texture with the current configuration
            return images.CreateEmptyTexture(format, createUav, createRtv);
        }

        /// <summary>
        /// creates a 2d srgb staging texture
        /// </summary>
        /// <param name="mipmap"></param>
        /// <returns></returns>
        public TextureArray2D Get2DSrgbStagingTexture(int mipmap)
        {
            Debug.Assert(mipmap < images.NumMipmaps);
            if(staging2D == null)
            {
                staging2D = new TextureArray2D[images.NumMipmaps];
            }
            if(staging2D[mipmap] == null)
            {
                // create new staging texture
                staging2D[mipmap] = new TextureArray2D(LayerMipmapCount.One, images.Size.GetMip(mipmap), Format.R8G8B8A8_UNorm_SRgb, false, true);
            }
            return staging2D[mipmap];
        }

        public void StoreTexture(ITexture tex)
        {
            Debug.Assert(tex != null);
            if (IsCompatibleWith(tex))
            {
                // can be used for later
                textures.Push(tex);
            }
            else
            {
                // immediately discard (incompatible image)
                tex.Dispose();
            }
        }

        public bool IsCompatibleWith(ITexture tex)
        {
            Debug.Assert(tex != null);
            return images.HasMatchingProperties(tex);
        }

        private void Clear()
        {
            foreach (var tex in textures)
            {
                tex.Dispose();
            }
            textures.Clear();
            
            if(staging2D != null)
            {
                foreach (var context in staging2D)
                {
                    context.Dispose();
                }
                staging2D = null;
            }
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ImagesModel.NumLayers) ||
                args.PropertyName == nameof(ImagesModel.NumMipmaps) ||
                args.PropertyName == nameof(ImagesModel.Size) ||
                args.PropertyName == nameof(ImagesModel.ImageType))
            {
                Clear();
                OnChanged();
            }
        }

        public void Dispose()
        {
            Clear();
            images.PropertyChanged -= ImagesOnPropertyChanged;
        }

        // event that will be called if the texture properties have changed
        public event EventHandler Changed;

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        // cache can only give valid images if the images model contains imagtes
        public bool IsValid => images.NumImages > 0;
    }
}
