using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace ImageFramework.Model.Shader
{
    public class ShaderBuilder2D : IShaderBuilder
    {
        public string SrvType => "Texture2DArray<float4>";
        public string SrvSingleType => "Texture2D<float4>";

        public string UavType => "RWTexture2DArray<float4>";

        public int LocalSizeX => 32;
        public int LocalSizeY => 32;
        public int LocalSizeZ => 1;

        public bool Is3D => false;

        public string Is3DString => "false";
    }
}
