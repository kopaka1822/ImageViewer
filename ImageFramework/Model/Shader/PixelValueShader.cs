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
        private readonly DirectX.Shader shader;
        private readonly DownloadBuffer<Color> readBuffer;
        private readonly UploadBuffer<PixelValueData> cbuffer;

        public PixelValueShader()
        {
            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(), "PixelValueShader");
            readBuffer = new DownloadBuffer<Color>();
            cbuffer = new UploadBuffer<PixelValueData>(1);
        }

        /// <summary>
        /// returns pixel color at the given position
        /// </summary>
        /// <param name="image">source image</param>
        /// <param name="x">x pixel coordinate</param>
        /// <param name="y">y pixel coordinate</param>
        /// <param name="layer">layer</param>
        /// <param name="mipmap">mipmap</param>
        /// <param name="radius">summation radius (0 = only this pixel)</param>
        /// <returns></returns>
        public Color Run(TextureArray2D image, int x, int y, int layer, int mipmap, int radius = 0)
        {
            cbuffer.SetData(new PixelValueData
            {
                PixelX = x,
                PixelY = y,
                Width = image.GetWidth(mipmap),
                Height = image.GetHeight(mipmap),
                Radius = radius
            });

            var dev = Device.Get();
            dev.Compute.Set(shader.Compute);
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

        private static string GetSource()
        {
            return $@"
Texture2D<float4> src_image : register(t0);

cbuffer InputBuffer : register(b0) {{
    int2 pixelCoord;
    int2 size;
    int radius;
}};

RWStructuredBuffer<float4> out_buffer : register(u0);

[numthreads(1, 1, 1)]
void main(){{
    float4 sum = float4(0.0, 0.0, 0.0, 0.0);
    for(int x = pixelCoord.x - radius; x <= pixelCoord.x + radius; ++x)
    for(int y = pixelCoord.y - radius; y <= pixelCoord.y + radius; ++y) {{
        int2 pixel = clamp(int2(x, y), int2(0, 0), size - int2(1, 1));
        sum += src_image[pixel];
    }}

    uint width = 1 + 2 * radius;
    float areaInv = 1.0f / float(width * width);
    out_buffer[0] = sum * areaInv;
}}
";
        }

        public void Dispose()
        {
            shader?.Dispose();
            readBuffer?.Dispose();
            cbuffer?.Dispose();
        }
    }
}
