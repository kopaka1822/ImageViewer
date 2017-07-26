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
        protected readonly uint target;
        public uint FilterMode { get; set; }

        public TextureBase(OpenGL gl, uint target)
        {
            this.gl = gl;
            this.target = target;
            this.FilterMode = OpenGL.GL_LINEAR;

            uint[] ids = new uint[1];
            gl.GenTextures(1, ids);
            this.id = ids[0];
        }

        public void Bind(uint slot)
        {
            gl.ActiveTexture(slot);
            gl.BindTexture(target, id);
            gl.TexParameter(target, OpenGL.GL_TEXTURE_MIN_FILTER, FilterMode);
            gl.TexParameter(target, OpenGL.GL_TEXTURE_MAG_FILTER, FilterMode);
        }
    }
}
