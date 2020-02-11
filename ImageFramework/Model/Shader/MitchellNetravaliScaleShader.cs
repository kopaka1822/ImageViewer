using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Model.Scaling;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    public class MitchellNetravaliScaleShader : IDisposable
    {
        private readonly DirectX.Shader shader;
        private readonly QuadShader quad;
        private readonly UploadBuffer cbuffer;

        public MitchellNetravaliScaleShader(QuadShader quad, UploadBuffer upload)
        {
            this.quad = quad;
            shader = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(), "MitchellNetravaliScale");
            cbuffer = upload;
        }

        public TextureArray2D Run(TextureArray2D src, Size3 dstSize, ScalingModel scaling)
        {
            Debug.Assert(src.Size != dstSize);
            var genMipmaps = src.NumMipmaps > 1;
            var numMipmaps = 1;
            if (genMipmaps)
                numMipmaps = dstSize.MaxMipLevels;

            bool changeWidth = dstSize.Width != src.Size.Width;
            bool changeHeight = dstSize.Height != src.Size.Height;

            if (changeWidth)
            {
                var curMips = numMipmaps;

                if (changeHeight) // only temporary texture with a single mipmap
                    curMips = 1;
                
                var tmp = new TextureArray2D(new LayerMipmapCount(src.NumLayers, curMips), new Size3(dstSize.Width, src.Size.Height), src.Format, false);               
                Apply(src, tmp, 1, 0);
                src = tmp;
            }

            if (changeHeight)
            {
                var tmp = new TextureArray2D(new LayerMipmapCount(src.NumLayers, numMipmaps), dstSize, src.Format, false);

                Apply(src, tmp, 0, 1);
                if (changeWidth) // delete temporary texture created by width invocation
                {
                    src.Dispose();
                }
                src = tmp;
            }

            if(genMipmaps) scaling.WriteMipmaps(src);

            return src;
        }

        private void Apply(TextureArray2D src, TextureArray2D dst, int dirX, int dirY)
        {
            quad.Bind(false);
            var dev = Device.Get();
            dev.Pixel.Set(shader.Pixel);

            cbuffer.SetData(new DirSizeData
            {
                DirX = dirX,
                DirY = dirY,
                SizeX = src.Size.Width,
                SizeY = src.Size.Height
            });

            dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
            dev.SetViewScissors(dst.Size.Width, dst.Size.Height);

            foreach (var lm in src.LayerMipmap.LayersOfMipmap(0))
            {
                dev.Pixel.SetShaderResource(0, src.GetSrView(lm));
                dev.OutputMerger.SetRenderTargets(dst.GetRtView(lm));
                dev.DrawQuad();
            }

            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            quad.Unbind();
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
    coords[2] = floor(centerX + 0.5) * dir + yOffset;
    coords[3] = floor(centerX + 1.5) * dir + yOffset;

    float4 colors[4];
    [unroll]
    for(int p = 0; p < 4; ++p)
        colors[p] = in_tex[clamp(int2(coords[p]), int2(0, 0), size - int2(1, 1))];

    // distance between center and coords[1]
    float d = centerX - floor(centerX - 0.5) - 0.5;

    // calculate color
    const float B = 0.0f;
    const float C = 0.5f;

    float4 res = ((-B/6.0-C) * colors[0] + (-3.0/2.0*B-C+2.0) * colors[1] + (3.0/2.0*B+C-2.0) * colors[2] + (B/6.0+C) * colors[3]) * d * d * d;
    res += ((B/2.0+2.0*C) * colors[0] + (2.0*B+C-3.0) * colors[1] + (-5.0/2.0*B-2.0*C+3.0) * colors[2] - C * colors[3]) * d * d;
    res += ((-B/2.0-C) * colors[0] + (B/2.0+C) * colors[2]) * d;
    res += B/6.0 * colors[0] + (-B/3.0+1.0) * colors[1] + B/6.0 * colors[2];

    return res;
}



";
        }
        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
