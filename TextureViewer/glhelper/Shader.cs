using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace TextureViewer.glhelper
{
    public class Shader
    {
        public int Id { get; private set; }
        public string Source { get; set; }
        private readonly ShaderType type;

        public Shader(ShaderType type)
        {
            this.type = type;
            Id = GL.CreateShader(type);
        }

        public Shader(ShaderType type, String source)
        {
            this.type = type;
            this.Source = source;
            Id = GL.CreateShader(type);
        }

        public Shader Compile()
        {
            GL.ShaderSource(Id, Source);
            GL.CompileShader(Id);

            int status;
            GL.GetShader(Id, ShaderParameter.CompileStatus, out status);

            if (status == 0)
                throw new Exception($"Error Compiling {type} Shader: {GL.GetShaderInfoLog(Id)}");

            return this;
        }

        public void Dispose()
        {
            if (Id != 0)
            {
                GL.DeleteShader(Id);
                Id = 0;
            }
        }
    }
}
