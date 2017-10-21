using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public class Program
    {
        private int id;

        public Program(List<Shader> shaders, bool deleteShaders)
        {
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

            foreach (var shader in shaders)
            {
                GL.DetachShader(id, shader.Id);
                if(deleteShaders)
                    shader.Dispose();
            }
        }

        public void Bind()
        {
            Debug.Assert(id != 0);
            GL.UseProgram(id);
        }

        public static void Unbind()
        {
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            if (id != 0)
            {
                GL.DeleteProgram(id);
                id = 0;
            }
        }
    }
}
