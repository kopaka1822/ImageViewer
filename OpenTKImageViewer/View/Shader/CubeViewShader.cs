using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.View.Shader
{
    public class CubeViewShader : ViewShader
    {
        public CubeViewShader() : 
            base(GetVertexSource(), GetFragmentSource())
        {
        }

        public override void Bind(ImageContext.ImageContext context)
        {
            ShaderProgram.Bind();
        }

        public void SetTransform(Matrix4 trans)
        {
            GL.UniformMatrix4(0, false, ref trans);
        }

        public void SetFarplane(float farplane)
        {
            GL.Uniform1(2, farplane);
        }

        public void SetLevel(float level)
        {
            GL.Uniform1(3, level);
        }

        public void SetGrayscale(ImageContext.ImageContext.GrayscaleMode mode)
        {
            GL.Uniform1(4, (uint)mode);
        }

        public int GetTextureLocation()
        {
            return 0;
        }

        public static string GetVertexSource()
        {
            return GetVersion() +
                   // uniforms
                   "layout(location = 0) uniform mat4 transform;\n" +
                   "layout(location = 2) uniform float farplane;\n" +
                   // out
                   "layout(location = 0) out vec3 viewDir;" +

                   "void main(void){\n" +
                   "vec4 vertex = vec4(0.0, 0.0, 0.0, 1.0);" +
                   "if(gl_VertexID == 0u) vertex = vec4(1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 1u) vertex = vec4(-1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 2u) vertex = vec4(1.0, 1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 3u) vertex = vec4(-1.0, 1.0, 0.0, 1.0);\n" +
                   "gl_Position = vertex;\n" +
                   "viewDir = (transform * vec4(vertex.xy, farplane, 0.0)).xyz;\n" +
                   "}\n";
        }

        public static string GetFragmentSource()
        {
            return GetVersion() +
                   // uniforms
                   "layout(binding = 0) uniform samplerCube tex;\n" +
                   "layout(location = 3) uniform float level;\n" +
                   "layout(location = 4) uniform uint grayscale;\n" +
                   // in out
                   "layout(location = 0) in vec3 viewdir;\n" +
                   "out vec4 fragColor;\n" +

                   "void main(void){\n" +
                   "vec4 color = textureLod(tex, viewdir, level);\n" +
                   ApplyGrayscale() +
                   "fragColor = color;\n" +
                   "}\n";
        }
    }
}
