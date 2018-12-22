using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;

namespace TextureViewer.Controller.TextureViews.Shader
{
    public class CheckersShader
    {
        private Program shaderProgram;

        public CheckersShader()
        {
            var shaders = new List<glhelper.Shader>(2)
            {
                new glhelper.Shader(ShaderType.VertexShader, GetVertexSource()).Compile(),
                new glhelper.Shader(ShaderType.FragmentShader, GetFragmentSource()).Compile()
            };

            shaderProgram = new Program(shaders, true);
        }

        /// <summary>
        /// bind checkers shader
        /// </summary>
        /// <param name="transform">transformation for the [-1, 1]x[-1, 1] plane</param>
        public void Bind(Matrix4 transform)
        {
            shaderProgram.Bind();
            SetTransform(transform);
        }

        public void Dispose()
        {
            shaderProgram?.Dispose();
            shaderProgram = null;
        }

        private void SetTransform(Matrix4 mat)
        {
            GL.UniformMatrix4(1, false, ref mat);
        }

        private static string GetVertexSource()
        {
            return "#version 430 core\n" +
                   "layout(location = 1) uniform mat4 transform;\n" +
                   "void main(void){\n" +
                   "vec4 vertex = vec4(0.0, 0.0, 0.0, 1.0);" +
                   "if(gl_VertexID == 0u) vertex = vec4(1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 1u) vertex = vec4(-1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 2u) vertex = vec4(1.0, 1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 3u) vertex = vec4(-1.0, 1.0, 0.0, 1.0);\n" +
                   "gl_Position = transform * vertex;\n" +
                   "}\n";
        }

        private static string GetFragmentSource()
        {
            return "#version 430 core\n" +
                   "out vec4 fragColor;\n" +
                   "void main(){\n" +
                   "ivec2 pixel = ivec2(gl_FragCoord.xy);\n" +
                   "pixel /= ivec2(10);\n" +
                   "bool isDark = ((pixel.x & 1) == 0);\n" +
                   "if( (pixel.y & 1) == 0 ) isDark = !isDark;\n" +
                   "float c = isDark? 0.7 : 0.5;\n" +
                   "fragColor = vec4(c,c,c,1.0);\n" +
                   "}\n";
        }
    }
}
