using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using ImageFramework.DirectX;
using ImageFramework.Model;
using SharpDX.DXGI;

namespace ImageFramework.Utility
{
    internal class TextureCache
    {
        private readonly Stack<ITexture> textures = new Stack<ITexture>(2);
        private readonly ImagesModel images;

        public TextureCache(ImagesModel images)
        {
            this.images = images;
            images.PropertyChanged += ImagesOnPropertyChanged;
        }

        /// <summary>
        /// returns one unused texture if available. creates a new texture if not textures were available
        /// </summary>
        /// <returns></returns>
        public ITexture GetTexture()
        {
            if (textures.Count > 0) return textures.Pop();

            // make new texture with the current configuration
            return images.CreateEmptyTexture();
        }

        /// <summary>
        /// stores the textures for later use
        /// </summary>
        /// <param name="tex"></param>
        public void StoreTexture(ITexture tex)
        {
            Debug.Assert(tex != null);
            if (images.HasMatchingProperties(tex))
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
            foreach (var tex in textures)
            {
                tex.Dispose();
            }
            textures.Clear();
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ImagesModel.NumLayers) ||
                args.PropertyName == nameof(ImagesModel.NumMipmaps) ||
                args.PropertyName == nameof(ImagesModel.Size) || 
                args.PropertyName == nameof(ImagesModel.ImageType))
                Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
