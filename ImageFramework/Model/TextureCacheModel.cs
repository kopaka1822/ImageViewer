using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using SharpDX.DXGI;

namespace ImageFramework.Model
{
    public class TextureCacheModel : IDisposable
    {
        private readonly Stack<TextureArray2D> textures = new Stack<TextureArray2D>(2);
        private readonly ImagesModel images;

        public TextureCacheModel(ImagesModel images)
        {
            this.images = images;
            images.PropertyChanged += ImagesOnPropertyChanged;
        }

        /// <summary>
        /// returns one unused texture if available. creates a new texture if not textures were available
        /// </summary>
        /// <returns></returns>
        public TextureArray2D GetTexture()
        {
            if (textures.Count > 0) return textures.Pop();

            // make new texture with the current configuration
            return new TextureArray2D(images.NumLayers, images.NumMipmaps,
                images.GetWidth(0), images.GetHeight(0), Format.R32G32B32A32_Float);
        }

        /// <summary>
        /// stores the textures for later use
        /// </summary>
        /// <param name="tex"></param>
        public void StoreTexture(TextureArray2D tex)
        {
            Debug.Assert(tex != null);
            if (tex.NumMipmaps == images.NumMipmaps && tex.NumMipmaps == images.NumLayers)
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

        /// <summary>
        /// disposes all textures
        /// </summary>
        private void Clear()
        {
            foreach (var textureArray2D in textures)
            {
                textureArray2D.Dispose();
            }
            textures.Clear();
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ImagesModel.NumLayers) ||
                args.PropertyName == nameof(ImagesModel.NumMipmaps) ||
                args.PropertyName == nameof(ImagesModel.Width) ||
                args.PropertyName == nameof(ImagesModel.Height))
                Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
