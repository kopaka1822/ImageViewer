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
    }
}
