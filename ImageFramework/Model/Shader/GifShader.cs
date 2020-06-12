using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    public class GifShader : IDisposable
    {
        private struct BufferData
        {
            public int BorderLocation;
            public int BorderSize;
            public RawBool UseOverlay;
        }

        private readonly DirectX.Shader pixel;

        public GifShader()
        {
            pixel = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(), "GifPixelShader");
        }

        public void Run(ShaderResourceView left, ShaderResourceView right, ShaderResourceView overlay, RenderTargetView dst, int borderSize, int borderLocation, int width, int height, QuadShader quad, UploadBuffer cbuffer)
        {
            Debug.Assert(borderLocation >= 0);
            Debug.Assert(borderLocation < width);

            quad.Bind(false);
            var dev = Device.Get();
            dev.Pixel.Set(pixel.Pixel);

            // update buffer
            cbuffer.SetData(new BufferData
            {
                BorderLocation = borderLocation,
                BorderSize = borderSize,
                UseOverlay = overlay != null
            });

            // bind and draw
            dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
            dev.Pixel.SetShaderResource(0, left);
            dev.Pixel.SetShaderResource(1, right);
            dev.Pixel.SetShaderResource(2, overlay);
            dev.OutputMerger.SetRenderTargets(dst);
            dev.SetViewScissors(width, height);
            dev.DrawQuad();

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetShaderResource(1, null);
            dev.Pixel.SetShaderResource(2, overlay);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            quad.Unbind();
        }

        private static string GetSource()
        {
            return @"
Texture2D<float4> left : register(t0);
Texture2D<float4> right : register(t1);
Texture2D<float4> overlay : register(t2);

cbuffer InfoBuffer : register(b0)
{
    int borderLocation;
    int borderSize;
    bool useOverlay;
};

struct PixelIn
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

float4 main(PixelIn i) : SV_TARGET
{
    int2 coord = int2(i.projPos.xy);
    float4 color;
    if(coord.x > borderLocation) color = right[coord];
    else color = left[coord];

    if(useOverlay)
    {
        // overlay has premultiplied color and alpha channel is (1-alpha)
        color = overlay[coord] + overlay[coord].a * color;
    }

    if(abs(coord.x - borderLocation) < borderSize)
        color = 0.0;
    
    color.a = 1.0;
    return color;
}
";
        }

        public void Dispose()
        {
            pixel?.Dispose();
        }
    }
}
