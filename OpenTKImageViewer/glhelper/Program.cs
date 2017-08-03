using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public class Program
    {
        private int id;
        private List<Shader> shaders;

        public Program(List<Shader> shaders)
        {
            this.shaders = shaders;
            id = GL.CreateProgram();

            foreach (var shader in shaders)
            {
                GL.AttachShader(id, shader.Id);
            }

            GL.LinkProgram(id);

            // verify
            int status;
            GL.GetProgram(id, GetProgramParameterName.LinkStatus, out status);
            if(status == 0)
                throw new Exception($"Error Linking Shader Programm: {GL.GetProgramInfoLog(id)}");
        }

        public void Bind()
        {
            GL.UseProgram(id);
        }
    }
}
