using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;
using Texture3D = ImageFramework.DirectX.Texture3D;

namespace ImageFramework.Model.Shader
{
    internal class ConvertTo3DShader : IDisposable
    {
        private readonly QuadShader quad;
        private readonly DirectX.Shader shader;

        public ConvertTo3DShader(QuadShader quad)
        {
            this.quad = quad;
            shader = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(), "ArrayTo3DShader");
        }

        public Texture3D ConvertTo3D(TextureArray2D src)
        {
            var dst = new Texture3D(1, new Size3(src.Size.X, src.Size.Y, src.NumLayers), Format.R32G32B32A32_Float,
                false);

            var dev = Device.Get();
            quad.Bind(true);
            dev.Pixel.Set(shader.Pixel);

            dev.Pixel.SetShaderResource(0, src.View);
            dev.OutputMerger.SetRenderTargets(dst.GetRtView(LayerMipmapSlice.Mip0));
            dev.SetViewScissors(dst.Size.Width, dst.Size.Height);
            dev.DrawFullscreenTriangle(dst.Size.Z);

            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);

            return dst;
        }


        public void Dispose()
        {
            shader?.Dispose();
        }

        private static string GetSource()
        {
            return @"
Texture2DArray<float4> tex : register(t0);

struct PixelIn
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
    uint depth : SV_RenderTargetArrayIndex;
};

float4 main(PixelIn i) : SV_TARGET {
    return tex.mips[0][uint3(i.projPos.xy, i.depth)];
}
";
        }

    }
}
