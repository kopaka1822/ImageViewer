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
    public class SingleViewShader : ViewShader
    {
        public SingleViewShader() :
            base(GetVertexSource(), GetFragmentSource())
        {

        }

        public override void Bind()
        {
            ShaderProgram.Bind();
        }

        public void SetCrop(ExportModel model, int layer)
        {
            SetCropCoordinates(5, model, layer);
        }

        public void SetTransform(Matrix4 mat)
        {
            GL.UniformMatrix4(1, false, ref mat);
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

        public int GetTextureLocation()
        {
            return 0;
        }

        public static string GetVertexSource()
        {
            return GetVersion() +
                   // uniforms
                   "layout(location = 1) uniform mat4 transform;\n" +
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
                   "texcoord.y = 1.0 - texcoord.y;\n" +
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
                   "layout(location = 5) uniform vec4 crop;\n" +
                   // in out
                   "layout(location = 0) in vec2 texcoord;\n" +
                   "out vec4 fragColor;\n" +

                   "void main(void){\n" +
                   "vec4 color = textureLod(tex, vec3(texcoord, layer), level);\n" +
                   ApplyGrayscale() +
                   ApplyColorCrop() +
                   "fragColor = color;\n" +
                   "}\n";
        }
    }
}
