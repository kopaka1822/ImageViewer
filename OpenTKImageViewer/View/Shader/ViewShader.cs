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

        protected static string GetVersion()
        {
            return "#version 430 core\n";
        }

        /// <summary>
        /// adds grayscale line to shader. color must be the current color and grayscale the grayscale mode
        /// </summary>
        /// <returns></returns>
        protected static string ApplyGrayscale()
        {
            return "if(grayscale == uint(1)) color = color.rrrr;\n" +
                   "else if(grayscale == uint(2)) color = color.gggg;\n" +
                   "else if(grayscale == uint(3)) color = color.bbbb;\n" +
                   "else if(grayscale == uint(4)) color = color.aaaa;\n";
        }

        public void Dispose()
        {
            ShaderProgram?.Dispose();
        }
    }
}