using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;

namespace OpenTKImageViewer.ImageContext
{
    /// <summary>
    /// texture cache for textures used in the tonemapping shader and combine shader
    /// </summary>
    public class TextureCache
    {
        private readonly ImageContext parent;
        private readonly Stack<TextureArray2D> textures = new Stack<TextureArray2D>(2);

        public TextureCache(ImageContext parent)
        {
            this.parent = parent;
        }

        public TextureArray2D GetAvailableTexture()
        {
            if (textures.Count > 0)
            {
                return textures.Pop();
            }
            return new TextureArray2D(parent.GetNumLayers(), parent.GetNumMipmaps(),
                SizedInternalFormat.Rgba32f, parent.GetWidth(0), parent.GetHeight(0));
        }

        public void StoreUnusuedTexture(TextureArray2D tex)
        {
            textures.Push(tex);
        }
    }
}
