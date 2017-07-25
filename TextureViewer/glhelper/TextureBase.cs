using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL;

namespace TextureViewer.glhelper
{
    class TextureBase
    {
        protected readonly OpenGL gl;
        protected readonly uint id;

        public TextureBase(OpenGL gl)
        {
            this.gl = gl;

            uint[] ids = new uint[1];
            gl.GenTextures(1, ids);
            this.id = ids[0];
        }

        protected static uint GetInternalFormat(ImageLoaderWrapper.Image image)
        {
            switch (image.NumComponents)
            {
                case 1:
                    return OpenGL.GL_RED;
                case 2:
                    return OpenGL.GL_RG;
                case 3:
                    return OpenGL.GL_RGB;
                case 4:
                    return OpenGL.GL_RGBA;
            }
            throw new Exception("invalid internal image format. component count: " + image.NumComponents);
        }

        protected static uint GetImageType(ImageLoaderWrapper.Image image)
        {
            if (image.IsIntegerFormat)
            {
                // TODO signed or unsigend?
                if (image.ComponentSize == 1)
                    return OpenGL.GL_UNSIGNED_BYTE;
                if (image.ComponentSize == 2)
                    return OpenGL.GL_UNSIGNED_SHORT;
                if (image.ComponentSize == 4)
                    return OpenGL.GL_UNSIGNED_INT;
            }
            else
            {
                if (image.ComponentSize == 4)
                    return OpenGL.GL_FLOAT;
            }
            throw new Exception("invalid image type. integer format: " + image.IsIntegerFormat + 
                ". component size: " + image.ComponentSize);
        }
    }
}
