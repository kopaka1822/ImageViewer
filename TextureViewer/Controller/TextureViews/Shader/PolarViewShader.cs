using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Models;
using TextureViewer.Models.Dialog;

namespace TextureViewer.Controller.TextureViews.Shader
{
    class PolarViewShader : ViewShader
    {
        public PolarViewShader() :
            base(GetVertexSource(), GetFragmentSource())
        {
        }

        public override void Bind()
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

        public void SetMipmap(float level)
        {
            GL.Uniform1(3, level);
        }

        public void SetGrayscale(DisplayModel.GrayscaleMode mode)
        {
            GL.Uniform1(4, (uint)mode);
        }

        public void SetFarplane(float farplane)
        {
            GL.Uniform1(5, farplane);
        }

        public int GetTextureLocation()
        {
            return 0;
        }

        public void SetCrop(ExportModel model, int layer)
        {
            SetCropCoordinates(6, model, layer);
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
                   "raydir = normalize((transform * vec4(vertex.xy, farplane, 0.0)).xyz);\n" +
                   "raydir.y *= -1.0;\n" + 
                   "}\n";
        }

        public static string GetFragmentSource()
        {
            return GetVersion() +
                   // uniforms
                   "layout(binding = 0) uniform sampler2DArray tex;\n" +
                   "layout(location = 2) uniform float layer;\n" +
                   "layout(location = 3) uniform float level;\n" +
                   "layout(location = 4) uniform uint grayscale;\n" +
                   "layout(location = 6) uniform vec4 crop;\n" +
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
                   "polarDirection.s = normalizedDirection.x == 0.0 ? " +
                        "PI/2*sign(normalizedDirection.z) :" +
                        "atan(normalizedDirection.z, normalizedDirection.x);\n" +
                   "polarDirection.s = polarDirection.s / (2*PI) + 0.25;\n" +
                   "if( polarDirection.s < 0.0) polarDirection.s += 1.0;\n" +
                   "vec4 color = textureLod(tex, vec3(polarDirection.st, layer), level);\n" +
                   ApplyGrayscale() +
                   ApplyColorCrop("polarDirection") +
                   "fragColor = color;\n" +
                   "}\n";
        }
    }
}
