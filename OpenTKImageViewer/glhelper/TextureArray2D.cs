using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public class TextureArray2D
    {
        private int id;

        public TextureArray2D(ImageLoader.Image image)
        {
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, id);

            // create storage
            var internalFormat = (SizedInternalFormat)image.OpenglInternalFormat;

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, image.GetNumMipmaps(),
                internalFormat, image.GetWidth(0),
                image.GetHeight(0), image.Layers.Count);

            if (image.IsCompressed)
            {
                for (int face = 0; face < image.Layers.Count; ++face)
                {
                    for (int level = 0; level < image.GetNumMipmaps(); ++level)
                    {
                        GL.CompressedTexSubImage3D(TextureTarget.Texture2DArray, level, 
                            0, 0, face, image.GetWidth(level), image.GetHeight(level),
                            1, (PixelFormat)image.OpenglInternalFormat, 
                            (int)image.Layers[face].Mipmaps[level].Size,
                            image.Layers[face].Mipmaps[level].Bytes);
                    }
                }
            }
            else
            {
                var pixelFormat = (PixelFormat) image.OpenglExternalFormat;
                var pixelType = (PixelType) image.OpenglType;

                for (int face = 0; face < image.Layers.Count; ++face)
                {
                    for (int level = 0; level < image.GetNumMipmaps(); ++level)
                    {
                        GL.TexSubImage3D(TextureTarget.Texture2DArray, level,
                            0, 0, face, image.GetWidth(level), image.GetHeight(level),
                            1, pixelFormat, pixelType, image.Layers[face].Mipmaps[level].Bytes);
                    }
                }
            }
            
            Utility.GLCheck();
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinLod, 0.0f);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxLod, (float)image.GetNumMipmaps());
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxLevel, image.GetNumMipmaps());
        }

        public void Bind(int slot)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            Utility.GLCheck();
            GL.BindTexture(TextureTarget.Texture2DArray, id);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Utility.GLCheck();
        }
    }
}
