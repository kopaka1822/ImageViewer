using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.View.Shader
{
    public class SingleViewShader : ViewShader
    {
        public SingleViewShader() : 
            base(GetVertexSource(), GetFragmentSource())
        {

        }

        public override void Bind(ImageContext.ImageContext context)
        {
            ShaderProgram.Bind();
        }

        public void SetTransform(Matrix4 mat)
        {
            GL.UniformMatrix4(0, false, ref mat);
        }

        public static string GetVertexSource()
        {
            return "#version 450 core\n" +
                   "layout(location = 0) uniform mat4 transform;" +
                   "void main(void){\n" +
                   "vec4 vertex = vec4(0.0, 0.0, 0.0, 1.0);" +
                   "if(gl_VertexID == 0u) vertex = vec4(1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 1u) vertex = vec4(-1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 2u) vertex = vec4(1.0, 1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 3u) vertex = vec4(-1.0, 0.5, 0.0, 1.0);\n" +
                   "gl_Position = transform * vertex;\n" +
                   "}\n";
        }

        public static string GetFragmentSource()
        {
            return "#version 450 core\n" +
                   "out vec4 color;\n" +
                   "void main(void){\n" +
                   "color = vec4(1.0);\n" +
                   "}\n";
        }
    }
}
