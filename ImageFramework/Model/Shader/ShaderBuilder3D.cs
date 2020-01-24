using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Shader
{
    public class ShaderBuilder3D : IShaderBuilder
    {
        private readonly string type;

        public ShaderBuilder3D(string type = "float4")
        {
            this.type = type;
        }

        public string SrvType => $"Texture3D<{type}>";
        public string SrvSingleType => SrvType;

        public string UavType => $"RWTexture3D<{type}>";

        public int LocalSizeX => 10;
        public int LocalSizeY => 10;
        public int LocalSizeZ => 10;
        public bool Is3D => true;

        public string Is3DString => "true";
        public int Is3DInt => 1;
    }
}
