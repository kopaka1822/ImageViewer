using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL;

namespace TextureViewer.glhelper
{
    class TextureCubeMap : TextureBase
    {
        public TextureCubeMap(OpenGL gl, ImageLoaderWrapper.Image image, int mipmap)
            : 
            base(gl, OpenGL.GL_TEXTURE_CUBE_MAP)
        {
            Debug.Assert(image.Layers.Count == 6);
            gl.BindTexture(OpenGL.GL_TEXTURE_CUBE_MAP, Id);
            Utility.GlCheck(gl);

            int width = image.Layers[0].Mipmaps[mipmap].Width;
            int height = image.Layers[0].Mipmaps[mipmap].Height;

            if (image.IsCompressed)
            {
                for (int face = 0; face < 6; ++face)
                {
                    gl.CompressedTexImage2D(OpenGL.GL_TEXTURE_CUBE_MAP_POSITIVE_X + (uint)face,
                        0, image.OpenglInternalFormat, width, height, 0,
                        (int)image.Layers[face].Mipmaps[mipmap].Size,
                        image.Layers[face].Mipmaps[mipmap].Bytes);
                }
            }
            else
            {
                for (int face = 0; face < 6; ++face)
                {
                    gl.TexImage2D(OpenGL.GL_TEXTURE_CUBE_MAP_POSITIVE_X + (uint) face, 0,
                        image.OpenglInternalFormat, width, height, 0,
                        image.OpenglExternalFormat, image.OpenglType,
                        image.Layers[face].Mipmaps[mipmap].Bytes);
                }
            }

            gl.TexParameter(OpenGL.GL_TEXTURE_2D_ARRAY, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D_ARRAY, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D_ARRAY, OpenGL.GL_TEXTURE_WRAP_R, OpenGL.GL_CLAMP_TO_EDGE);
        }
    }
}
