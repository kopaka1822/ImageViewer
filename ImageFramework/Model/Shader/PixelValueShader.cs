using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Utility;

namespace ImageFramework.Model.Shader
{
    public class PixelValueShader : IDisposable
    {
        private readonly DirectX.Shader shader2D;
        private readonly DirectX.Shader shader3D;
        private readonly DownloadBuffer<Color> readBuffer;
        private readonly UploadBuffer<PixelValueData> cbuffer;

        public PixelValueShader()
        {
            shader2D = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(ShaderBuilder.Builder2D), "PixelValueShader");
            shader3D = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(ShaderBuilder.Builder3D), "PixelValueShader");
            readBuffer = new DownloadBuffer<Color>();
            cbuffer = new UploadBuffer<PixelValueData>(1);
        }

        /// <summary>
        /// returns pixel color at the given position
        /// </summary>
        /// <param name="image">source image</param>
        /// <param name="coord">x pixel coordinate</param>
        /// <param name="layer">layer</param>
        /// <param name="mipmap">mipmap</param>
        /// <param name="radius">summation radius (0 = only this pixel)</param>
        /// <returns></returns>
        public Color Run(ITexture image, Size3 coord, int layer, int mipmap, int radius = 0)
        {
            var dim = image.Size.GetMip(mipmap);

            cbuffer.SetData(new PixelValueData
            {
                PixelX = coord.X,
                PixelY = coord.Y,
                PixelZ = coord.Z,
                SizeX = dim.Width,
                SizeY = dim.Height,
                SizeZ = dim.Depth,
                Radius = radius
            });

            var dev = Device.Get();
            if(image.Is3D) dev.Compute.Set(shader3D.Compute);
            else dev.Compute.Set(shader2D.Compute);
            dev.Compute.SetConstantBuffer(0, cbuffer.Handle);
            dev.Compute.SetShaderResource(0, image.GetSrView(layer, mipmap));
            dev.Compute.SetUnorderedAccessView(0, readBuffer.Handle);

            dev.Dispatch(1, 1);

            // unbind textures
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);

            // obtain data
            return readBuffer.GetData();
        }

        private static string GetSource(IShaderBuilder builder)
        {
            return $@"
{builder.SrvSingleType} src_image : register(t0);

cbuffer InputBuffer : register(b0) {{
    int3 pixelCoord;
    int radius;
    int3 size;
}};

RWStructuredBuffer<float4> out_buffer : register(u0);

[numthreads(1, 1, 1)]
void main(){{
    float4 sum = float4(0.0, 0.0, 0.0, 0.0);
    
    {(builder.Is3D?"for(int z = pixelCoord.z - radius; z <= pixelCoord.z + radius; ++z)":"int z = 0;")}
    for(int x = pixelCoord.x - radius; x <= pixelCoord.x + radius; ++x)
    for(int y = pixelCoord.y - radius; y <= pixelCoord.y + radius; ++y) {{
        int3 pixel = clamp(int3(x, y, z), int3(0, 0, 0), size - int3(1, 1, 1));
        {(builder.Is3D? 
                "sum += src_image[pixel];" : 
                "sum += src_image[pixel.xy];")}
        
    }}

    uint width = 1 + 2 * radius;
    float areaInv = 1.0f / float(width * width {(builder.Is3D?"* width":"")});
    out_buffer[0] = sum * areaInv;
}}
";
        }

        public void Dispose()
        {
            shader2D?.Dispose();
            shader3D?.Dispose();
            readBuffer?.Dispose();
            cbuffer?.Dispose();
        }
    }
}
