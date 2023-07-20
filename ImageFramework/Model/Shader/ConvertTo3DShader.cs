using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
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
        private readonly DirectX.Shader shader3D;
        private readonly DirectX.Shader shaderLayer;

        public ConvertTo3DShader(QuadShader quad)
        {
            this.quad = quad;
            shader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource3D(), "ArrayTo3DShader");
            shaderLayer = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSourceLayer(), "3DToArrayShader");
        }

        public Texture3D ConvertTo3D(TextureArray2D src)
        {
            Debug.Assert(ImageFormat.IsSupported(src.Format));
            var dst = new Texture3D(1, new Size3(src.Size.X, src.Size.Y, src.NumLayers), src.Format,
                false);

            var dev = Device.Get();
            quad.Bind(true);
            dev.Pixel.Set(shader3D.Pixel);

            dev.Pixel.SetShaderResource(0, src.View);
            dev.OutputMerger.SetRenderTargets(dst.GetRtView(LayerMipmapSlice.Mip0));
            dev.SetViewScissors(dst.Size.Width, dst.Size.Height);
            dev.DrawFullscreenTriangle(dst.Size.Z);

            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            quad.Unbind();

            return dst;
        }

        public TextureArray2D ConvertToArray(Texture3D src, int fixedAxis1, int fixedAxis2, UploadBuffer cbuffer, int startLayer = 0, int numLayers = -1)
        {
            Debug.Assert(fixedAxis1 >= 0 && fixedAxis1 <= 2);
            Debug.Assert(fixedAxis2 >= 0 && fixedAxis2 <= 2);
            var dim = src.Size;
            var layerAxis = 3 - fixedAxis1 - fixedAxis2;

            if (numLayers < 0)
                numLayers = dim[layerAxis] - startLayer;

            Debug.Assert(ImageFormat.IsSupported(src.Format));
            var dst = new TextureArray2D(
                new LayerMipmapCount(numLayers, 1), 
                new Size3(dim[fixedAxis1], dim[fixedAxis2]),
                src.Format, false
            );

            var data = new LayerBufferData
            {
                XAxis = fixedAxis1,
                YAxis = fixedAxis2
            };

            var dev = Device.Get();
            quad.Bind(false);
            dev.Pixel.Set(shaderLayer.Pixel);
            dev.Pixel.SetShaderResource(0, src.GetSrView(0));
            dev.SetViewScissors(dst.Size.Width, dst.Size.Height);

            foreach (var lm in dst.LayerMipmap.Range)
            {
                data.ZValue = lm.Layer + startLayer;
                cbuffer.SetData(data);
                dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
                dev.OutputMerger.SetRenderTargets(dst.GetRtView(lm));

                dev.DrawFullscreenTriangle(1);
            }

            quad.Unbind();
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);

            return dst;
        }

        public void Dispose()
        {
            shader3D?.Dispose();
            shaderLayer?.Dispose();
        }

        private static string GetSource3D()
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

        private struct LayerBufferData
        {
            public int XAxis;
            public int YAxis;
            public int ZValue;
        }

        private static string GetSourceLayer()
        {
            return @"
Texture3D<float4> tex: register(t0);

cbuffer InfoBuffer : register(b0)
{
    int xaxis;
    int yaxis;
    int zvalue;
};

struct PixelIn
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

uint3 getCoord(uint2 coord){{
    uint res[3];
    res[xaxis] = coord.x;
    res[yaxis] = coord.y;
    // convert zvalue to float
    res[3 - xaxis - yaxis] = zvalue;
  
    return uint3(res[0], res[1], res[2]);
}}

float4 main(PixelIn i) : SV_TARGET {
    return tex[getCoord(i.projPos.xy)];
}
";
        }
    }
}
