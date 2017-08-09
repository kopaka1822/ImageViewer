using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.View.Shader
{
    public class PolarViewShader : ViewShader
    {
        public PolarViewShader() :
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

        public void SetLayer(float layer)
        {
            GL.Uniform1(2, layer);
        }

        public void SetLevel(float level)
        {
            GL.Uniform1(3, level);
        }

        public void SetGrayscale(ImageContext.ImageContext.GrayscaleMode mode)
        {
            GL.Uniform1(4, (uint)mode);
        }

        public void SetFarplane(float farplane)
        {
            GL.Uniform1(5, farplane);
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
                   "layout(location = 5) uniform float farplane;\n" +
                   // out
                   "layout(location = 0) out vec3 raydir;" +

                   "void main(void){\n" +
                   "vec4 vertex = vec4(0.0, 0.0, 0.0, 1.0);" +
                   "if(gl_VertexID == 0u) vertex = vec4(1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 1u) vertex = vec4(-1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 2u) vertex = vec4(1.0, 1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 3u) vertex = vec4(-1.0, 1.0, 0.0, 1.0);\n" +
                   "gl_Position = vertex;" +
                   "raydir = (transform * vec4(vertex.xy, farplane, 0.0)).xyz;\n" +
                   "}\n";
        }

        public static string GetFragmentSource()
        {
            return GetVersion() +
                   // uniforms
                   "layout(location = 1) uniform sampler2DArray tex;\n" +
                   "layout(location = 2) uniform float layer;\n" +
                   "layout(location = 3) uniform float level;\n" +
                   "layout(location = 4) uniform uint grayscale;\n" +
                   // in out
                   "layout(location = 0) in vec3 raydir;\n" +
                   "out vec4 fragColor;\n" +

                   "const float PI = 3.14159265358979323846264;" +

                   "void main(void){\n" +
                   "vec2 polarDirection;\n" +
                   // t computation
                   "vec3 normalizedDirection = normalize(raydir);\n" +
                   "polarDirection.t = acos(normalizedDirection.y)/PI;\n" +
                   // s computation
                   "normalizedDirection  = normalize(vec3(raydir.x, 0.0, raydir.z));\n" +

                   "if(normalizedDirection.x >= 0)\n" +
                   "polarDirection.s = acos(-normalizedDirection.z)/(2*PI);\n" +
                   "else\n" +
                   "polarDirection.s = (acos(normalizedDirection.z) + PI)/(2*PI);\n" +

                   "vec4 color = textureLod(tex, vec3(polarDirection.st, layer), level);\n" +
                   ApplyGrayscale() +
                   "fragColor = color;\n" +
                   "}\n";
        }
    }
}
