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

        public int GetTextureLocation()
        {
            return 1;
        }

        public static string GetVertexSource()
        {
            return GetVersion() +
                   // uniforms
                   "layout(location = 0) uniform mat4 transform;\n" +
                   // out
                   "layout(location = 0) out vec2 texcoord;" +

                   "void main(void){\n" +
                   "vec4 vertex = vec4(0.0, 0.0, 0.0, 1.0);" +
                   "if(gl_VertexID == 0u) vertex = vec4(1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 1u) vertex = vec4(-1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 2u) vertex = vec4(1.0, 1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 3u) vertex = vec4(-1.0, 1.0, 0.0, 1.0);\n" +
                   "gl_Position = transform * vertex;\n" +
                   // [-1,1]->[0,1]
                   "texcoord = (vertex.xy + vec2(1.0)) * vec2(0.5);\n" +
                   "}\n";
        }

        public static string GetFragmentSource()
        {
            return GetVersion() +
                   // uniforms
                   "layout(location = 1) uniform sampler2DArray tex;\n" +
                   // in out
                   "layout(location = 0) in vec2 texcoord;\n" +
                   "out vec4 fragColor;\n" +

                   "void main(void){\n" +
                   "vec4 color = texture(tex, vec3(texcoord, 0.0));\n" +
                   "fragColor = color;\n" +
                   "}\n";
        }
    }
}
