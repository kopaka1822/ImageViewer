using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpGL;

namespace TextureViewer.glhelper
{
    class TextureArray2D : TextureBase
    {
        public TextureArray2D(OpenGL gl, ImageLoaderWrapper.Image image, int mipmap) 
            : base(gl, OpenGL.GL_TEXTURE_2D_ARRAY)
        {
            int width = image.Layers[0].Mipmaps[mipmap].Width;
            int height = image.Layers[0].Mipmaps[mipmap].Height;

            gl.BindTexture(OpenGL.GL_TEXTURE_2D_ARRAY, id);
            
            // allocate memory
            gl.TexImage3D(OpenGL.GL_TEXTURE_2D_ARRAY,
                0, (int) image.OpenglInternalFormat, width, height,
                image.Layers.Count, 0, image.OpenglExternalFormat,
                image.OpenglType, IntPtr.Zero);

            // upload layers
            for (int curLayer = 0; curLayer < image.Layers.Count; ++curLayer)
            {
                gl.TexSubImage3D(OpenGL.GL_TEXTURE_2D_ARRAY,
                    0, 0, 0, curLayer, width, height, 1,
                    image.OpenglExternalFormat, image.OpenglType,
                    image.Layers[curLayer].Mipmaps[mipmap].Bytes);
            }

            gl.TexParameter(OpenGL.GL_TEXTURE_2D_ARRAY, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D_ARRAY, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);
        }
    }
}
