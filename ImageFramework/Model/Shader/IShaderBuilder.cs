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
        string SrvType { get; }
        string UavType { get; }

        int LocalSizeX { get; }
        int LocalSizeY { get; }
        int LocalSizeZ { get; }
        bool Is3D { get; }

        string Is3DString { get; }
    }

    public static class ShaderBuilder
    {
        public static IShaderBuilder Get(Type type)
        {
            if(type == typeof(TextureArray2D)) return new ShaderBuilder2D();
            if(type == typeof(Texture3D)) return new ShaderBuilder3D();
            throw new Exception("no shader builder available for " + type.Name);
        }
    }
}
