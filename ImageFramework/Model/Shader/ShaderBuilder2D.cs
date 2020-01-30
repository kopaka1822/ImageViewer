using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using ImageFramework.DirectX;

namespace ImageFramework.Model.Shader
{
    public class ShaderBuilder2D : IShaderBuilder
    {
        private readonly string type;

        public ShaderBuilder2D(string type = "float4")
        {
            this.type = type;
        }

        public string SrvType => $"Texture2DArray<{type}>";
        public string SrvSingleType => $"Texture2D<{type}>";

        public string UavType => $"RWTexture2DArray<{type}>";

        public int LocalSizeX => 32;
        public int LocalSizeY => 32;
        public int LocalSizeZ => 1;

        public bool Is3D => false;

        public string Is3DString => "false";
        public int Is3DInt => 0;

        public string TexelHelperFunctions => @"
int2 texel(int3 coord) {{ return coord.xy; }}
int3 texel(int3 coord, int layer) {{ return uint3(coord.xy, layer); }}
";

        public string Double => Device.Get().SupportsDouble ? "double" : "float";
        public string IntVec => "int2";
    }
}
