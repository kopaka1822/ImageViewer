using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Shader
{
    public class ShaderBuilder3D : IShaderBuilder
    {
        public string SrvType => "Texture3D<float4>";

        public string UavType => "RWTexture3D<float4>";

        public int LocalSizeX => 10;
        public int LocalSizeY => 10;
        public int LocalSizeZ => 10;
        public bool Is3D => true;

        public string Is3DString => "true";
    }
}
