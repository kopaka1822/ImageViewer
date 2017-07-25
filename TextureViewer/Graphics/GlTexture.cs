using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SharpGL;
using StbSharp;

namespace TextureViewer.Graphics
{
    class GlTexture
    {
        private OpenGL gl;
        private uint id;

        public GlTexture(OpenGL gl, string file)
        {
            this.gl = gl;

            // TODO deprecated
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            uint[] textures = new uint[1];
            gl.GenTextures(1, textures);
            id = textures[0];

            // TODO use appropriate binding
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, id);

            /*gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGB, bitmap.Width, bitmap.Height, 0, OpenGL.GL_RGB, OpenGL.GL_UNSIGNED_BYTE,
                bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb).Scan0);*/
            LoadStb(file);

            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
        }

        public void Bind()
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, id);
        }

        private bool LoadStb(string file)
        {
            byte[] buffer = File.ReadAllBytes(file);
            var image = Stb.LoadFromMemory(buffer, 0);
            
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGB, image.Width, image.Height, 0,
                OpenGL.GL_RGB, OpenGL.GL_UNSIGNED_BYTE, image.Data);
            
            

            return true;
        }
    }
}
