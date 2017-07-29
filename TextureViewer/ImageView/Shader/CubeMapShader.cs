using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.ImageView.Shader
{
    class CubeMapShader : Shader
    {
        public CubeMapShader(Context context) : base(context)
        {
        }

        protected override string GetFragmentShaderCode()
        {
            return GetVersion()
                   + GetVaryings()
                   + GetUniforms()
                   + GetTexturesCubeMap()
                   + GetTextureCubeMapsGetters()
                   + GetFinalColor()
                   + GetMain();
        }

        protected string GetVaryings()
        {
            return "in vec3 texcoord;\n";
        }

        protected string GetTexturesCubeMap()
        {
            return GetTextures("samplerCube");
        }

        protected string GetTextureCubeMapsGetters()
        {
            // TODO apply tone mapping function here?
            string res = "";
            for (int i = 0; i < context.GetNumImages(); ++i)
            {
                res += "vec4 GetTextureColor" + i + "(){\n";
                // image function
                res += "return texture(tex" + i + ", texcoord);\n";
                res += "}\n";
            }
            return res;
        }

        protected override string GetVertexShaderCode()
        {
            return GetVersion() +
                "in vec3 vertex;\n" +
                "out vec3 texcoord;\n" +
                "uniform mat4 modelMatrix;\n" +
                "void main(){\n" +
                   "texcoord = (modelMatrix * vec3(vertex.xy,1.0,0.0)).xyz;\n" +
                   "gl_Position = vec4(vertex.xy, 0.0, 1.0);\n" +
                "}";
        }
    }
}
