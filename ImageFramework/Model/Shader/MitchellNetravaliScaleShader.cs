using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    public class MitchellNetravaliScaleShader : IDisposable
    {
        private readonly DirectX.Shader shader;
        private readonly QuadShader quad;
        private readonly UploadBuffer<DirSizeData> cbuffer;

        public MitchellNetravaliScaleShader()
        {
            quad = new QuadShader();
            shader = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(), "MitchellNetravaliScale");
            cbuffer = new UploadBuffer<DirSizeData>(1);
        }

        public TextureArray2D Run(TextureArray2D src, int dstWidth, int dstHeight)
        {
            Debug.Assert(dstWidth != src.Width || dstHeight != src.Height);
            var genMipmaps = src.HasMipmaps;
            var numMipmaps = 1;
            if (genMipmaps)
                numMipmaps = ImagesModel.ComputeMaxMipLevels(dstWidth, dstHeight);

            bool changeWidth = dstWidth != src.Width;
            bool changeHeight = dstHeight != src.Height;

            if (changeWidth)
            {
                var curMips = numMipmaps;

                if (changeHeight) // only temporary texture with a single mipmap
                    curMips = 1;
                
                var tmp = new TextureArray2D(src.NumLayers, curMips, dstWidth, src.Height, src.Format, false);               
                Apply(src, tmp, 1, 0);
                src = tmp;
            }

            if (changeHeight)
            {
                var tmp = new TextureArray2D(src.NumLayers, numMipmaps, dstWidth, dstHeight, src.Format, false);

                Apply(src, tmp, 0, 1);
                if (changeWidth) // delete temporary texture created by width invocation
                {
                    src.Dispose();
                }
                src = tmp;
            }

            if(genMipmaps) src.RegenerateMipmapLevels();

            return src;
        }

        private void Apply(TextureArray2D src, TextureArray2D dst, int dirX, int dirY)
        {
            var dev = Device.Get();
            dev.Vertex.Set(quad.Vertex);
            dev.Pixel.Set(shader.Pixel);

            cbuffer.SetData(new DirSizeData
            {
                DirX = dirX,
                DirY = dirY,
                SizeX = src.Width,
                SizeY = src.Height
            });

            dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
            dev.SetViewScissors(dst.Width, dst.Height);

            for (var curLayer = 0; curLayer < src.NumLayers; ++curLayer)
            {
                dev.Pixel.SetShaderResource(0, src.GetSrView(curLayer, 0));
                dev.OutputMerger.SetRenderTargets(dst.GetRtView(curLayer, 0));
                dev.DrawQuad();
            }

            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
        }

        private static string GetSource()
        {
            return @"
Texture2D<float4> in_tex : register(t0);

cbuffer InfoBuffer : register(b0)
{
    int2 direction;
    int2 size;
};

struct PixelIn
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

float4 main(PixelIn i) : SV_TARGET
{
    float2 dir = float2(direction);
    float2 invDir = float2(int2(1, 1) - direction);

    float2 center = float2(i.texcoord * size);
    // we only filter in one direction defined by dir
    float centerX = dot(center, dir);
    float centerY = dot(center, invDir);
    float2 yOffset = centerY * invDir;    

    // get the 4 sample points
    int2 coords[4];
    coords[0] = floor(centerX - 1.5) * dir + yOffset;
    coords[1] = floor(centerX - 0.5) * dir + yOffset;
    coords[2] = floor(centerX + 1.5) * dir + yOffset;
    coords[3] = floor(centerX + 2.5) * dir + yOffset;

    float4 colors[4];
    [unroll]
    for(int p = 0; p < 4; ++p)
        colors[p] = in_tex[clamp(int2(coords[p]), int2(0, 0), size - int2(1, 1))];

    // distance between center and coords[1]
    float d = centerX - floor(centerX - 0.5);

    // calculate color
    const float B = 0.0f;
    const float C = 0.5f;

    float4 res = ((-B/6.0-C) * colors[0] + (-B/3.0-C+2.0) * colors[1] + (3.0/2.0*B+C-2.0) * colors[2] + (B/6.0+C) * colors[3]) * d * d * d;
    res += ((1.0/2.0*B+2.0*C) * colors[0] + (2.0*B+C-3.0) * colors[1] + (-5.0/2.0*B-2.0*C+3.0) * colors[2] - C * colors[3]) * d * d;
    res += ((-B/2.0-C) * colors[0] + (B/2.0+C) * colors[2]) * d;
    res += B/6.0 * colors[0] + (-B/3.0+1.0) * colors[1] + B/6.0 * colors[2];

    return res;
}



";
        }
        public void Dispose()
        {
            shader?.Dispose();
            quad?.Dispose();
        }
    }
}
