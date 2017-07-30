using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.ImageView.Shader
{
    class SingleViewShader : Shader
    {
        public SingleViewShader(Context context) : base(context)
        {
        }

        protected override string GetFragmentShaderCode()
        {
            return GetVersion()
                   + GetVaryings()
                   + GetUniforms()
                   + GetTextures2DArray()
                   + GetTexture2DArrayGetters()
                   + GetFinalColor()
                   + GetMain();
        }

        protected string GetVaryings()
        {
            return "in vec2 texcoord;\n" +
                   "out vec4 fragColor;\n";
        }

        protected string GetTexture2DArrayGetters()
        {
            // TODO apply tone mapping function here?
            string res = "";
            for (int i = 0; i < context.GetNumImages(); ++i)
            {
                res += "vec4 GetTextureColor" + i + "(){\n";
                // image function
                res += "return texture(tex" + i + ", vec3(texcoord, float(currentLayer)));\n";
                res += "}\n";
            }
            return res;
        }

        protected string GetTextures2DArray()
        {
            return GetTextures("sampler2DArray");
        }

        protected override string GetVertexShaderCode()
        {
            return GetVersion() +
                   "in vec4 vertex;\n" +
                   "out vec2 texcoord;\n" +
                   "uniform mat4 modelMatrix;\n" +
                   "void main(){\n" +
                   "texcoord = (vertex.xy + vec2(1.0)) * vec2(0.5);\n" +
                   "gl_Position = modelMatrix * vec4(vertex.xy, 0.0, 1.0);\n" +
                   "}";
        }
    }
}
