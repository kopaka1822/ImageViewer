using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;

namespace ImageFramework.Model.Shader
{
    public interface IShaderBuilder
    {
        // all mipmaps, all layers
        string SrvType { get; }
        // single mipmap, single layer
        string SrvSingleType { get; }
        string UavType { get; }

        int LocalSizeX { get; }
        int LocalSizeY { get; }
        int LocalSizeZ { get; }
        bool Is3D { get; }

        string Is3DString { get; }

        int Is3DInt { get; }

        string TexelHelperFunctions { get; }

        // doubles are not supported on all devices => this is either "float" or "double" based on the hardware
        string Double { get; }

        // either int2 or int3
        string IntVec { get; }
        
        // underlying texture type (float4 etc.)
        string Type { get; }
    }

    public static class ShaderBuilder
    {
        public static IShaderBuilder Get(Type type)
        {
            if(type == typeof(TextureArray2D)) return Builder2D;
            if(type == typeof(Texture3D)) return Builder3D;
            throw new Exception("no shader builder available for " + type.Name);
        }

        public static readonly IShaderBuilder Builder2D = new ShaderBuilder2D();
        public static readonly IShaderBuilder Builder3D = new ShaderBuilder3D();
    }
}
