using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;

namespace ImageFramework.Model.Shader
{
    public class ShaderBuilder3D : IShaderBuilder
    {
        public string Type { get; }

        public ShaderBuilder3D(string type = "float4")
        {
            this.Type = type;
        }

        public string SrvType => $"Texture3D<{Type}>";
        public string SrvSingleType => SrvType;

        public string UavType => $"RWTexture3D<{Type}>";

        public int LocalSizeX => Device.Get().IsLowEndDevice ? 6 : 10;
        public int LocalSizeY => Device.Get().IsLowEndDevice ? 6 : 10;
        public int LocalSizeZ => Device.Get().IsLowEndDevice ? 6 : 10;
        
        public bool Is3D => true;

        public string Is3DString => "true";
        public int Is3DInt => 1;

        public string TexelHelperFunctions => @"
int3 texel(int3 coord) {{ return coord; }}
int3 texel(int3 coord, int layer) {{ return coord; }}
";

        public string Double => Device.Get().SupportsDouble ? "double" : "float";
        public string IntVec => "int3";
    }
}
