using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;
using TextureViewer.Models;

namespace TextureViewer.Controller.TextureViews.Shader
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

        public abstract void Bind();

        protected static string GetVersion()
        {
            return OpenGlContext.ShaderVersion + "\n";
        }

        /// <summary>
        /// adds grayscale line to shader. color must be the current color and grayscale the grayscale mode
        /// </summary>
        /// <returns></returns>
        protected static string ApplyGrayscale()
        {
            return "if(grayscale == uint(1)) color = vec4(color.rrr,1.0);\n" +
                   "else if(grayscale == uint(2)) color = vec4(color.ggg,1.0);\n" +
                   "else if(grayscale == uint(3)) color = vec4(color.bbb,1.0);\n" +
                   "else if(grayscale == uint(4)) color = vec4(color.aaa,1.0);\n";
        }

        public void Dispose()
        {
            ShaderProgram?.Dispose();
        }
    }
}
