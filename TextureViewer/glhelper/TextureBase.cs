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
        // ReSharper disable once InconsistentNaming
        protected readonly OpenGL gl;
        protected readonly uint Id;
        protected readonly uint Target;
        public uint FilterMode { get; set; }

        public TextureBase(OpenGL gl, uint target)
        {
            this.gl = gl;
            this.Target = target;
            this.FilterMode = OpenGL.GL_LINEAR;

            uint[] ids = new uint[1];
            gl.GenTextures(1, ids);
            this.Id = ids[0];
        }

        public void Bind(uint slot)
        {
            gl.ActiveTexture(OpenGL.GL_TEXTURE0 + slot);
            Utility.GlCheck(gl);
            gl.BindTexture(Target, Id);
            gl.TexParameter(Target, OpenGL.GL_TEXTURE_MIN_FILTER, FilterMode);
            gl.TexParameter(Target, OpenGL.GL_TEXTURE_MAG_FILTER, FilterMode);
            Utility.GlCheck(gl);
        }
    }
}
