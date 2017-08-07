using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;

namespace OpenTKImageViewer.View.Shader
{
    public abstract class ViewShader
    {
        protected Program ShaderProgram;

        protected ViewShader(string vertexSource, string fragmentSource)
        {
            List<glhelper.Shader> shaders = new List<glhelper.Shader>(2);
            shaders.Add(new glhelper.Shader(ShaderType.VertexShader, vertexSource).Compile());
            shaders.Add(new glhelper.Shader(ShaderType.FragmentShader, fragmentSource).Compile());

            ShaderProgram = new Program(shaders, true);
        }

        public abstract void Bind(ImageContext.ImageContext context);
    }
}
