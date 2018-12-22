using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;

namespace TextureViewer.Models
{
    public class TextureCacheModel
    {
        private readonly Stack<TextureArray2D> textures = new Stack<TextureArray2D>(2);
        private readonly ImagesModel images;
        private readonly OpenGlContext glContext;

        public TextureCacheModel(ImagesModel images, OpenGlContext glContext)
        {
            this.images = images;
            this.glContext = glContext;

            images.PropertyChanged += ImagesOnPropertyChanged;
        }

        /// <summary>
        /// returns one unused texture if available. creates a new texture if not textures were available
        /// </summary>
        /// <returns></returns>
        public TextureArray2D GetTexture()
        {
            if (textures.Count > 0) return textures.Pop();

            Debug.Assert(glContext.GlEnabled);
            // make new texture with the current configuration
            return new TextureArray2D(images.NumLayers, images.NumMipmaps,
                SizedInternalFormat.Rgba32f, images.GetWidth(0), images.GetHeight(0));
        }

        /// <summary>
        /// stores the textures for later use
        /// </summary>
        /// <param name="tex"></param>
        public void StoreTexture(TextureArray2D tex)
        {
            Debug.Assert(tex != null);
            if(tex.NumMipmaps == images.NumMipmaps)
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
        public void Clear()
        {
            Debug.Assert(glContext.GlEnabled);

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
    }
}
