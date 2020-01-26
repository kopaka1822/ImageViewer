using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Scaling.Down
{
    // fast gauss shader for detail preserving downscaling
    internal class FastGaussShader : IDisposable
    {
        private QuadShader quad;
        private DirectX.Shader shader = null;
        private DirectX.Shader shader3D = null;

        public FastGaussShader(QuadShader quad)
        {
            this.quad = quad;
        }

        private DirectX.Shader Shader => shader ?? (shader = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                             GetSource(ShaderBuilder.Builder2D), "FastGauss"));

        private DirectX.Shader Shader3D => shader3D ?? (shader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                             GetSource(ShaderBuilder.Builder3D), "FastGauss3D"));

        private struct BufferData
        {
            public Size3 Size;
            public int HasAlpha;
        }

        // executes a 3x3 gauss kernel on the given texture mipmap
        public void Run(ITexture texSrc, ITexture texDst, int mipmap, bool hasAlpha, UploadBuffer upload)
        {
            Debug.Assert(texSrc.NumLayers == texDst.NumLayers);
            Debug.Assert(texSrc.NumMipmaps == texDst.NumMipmaps);
            Debug.Assert(texSrc.Size == texDst.Size);

            var dev = Device.Get();
            quad.Bind(texSrc.Is3D);
            dev.Pixel.Set(texSrc.Is3D ? Shader3D.Pixel : Shader.Pixel);
            
            var data = new BufferData
            {
                Size = texSrc.Size.GetMip(mipmap),
                HasAlpha = hasAlpha?1:0,
            };

            for (int layer = 0; layer < texSrc.NumLayers; ++layer)
            {
                upload.SetData(data);
                dev.Pixel.SetConstantBuffer(0, upload.Handle);
                dev.Pixel.SetShaderResource(0, texSrc.GetSrView(layer, mipmap));

                dev.OutputMerger.SetRenderTargets(texDst.GetRtView(layer, mipmap));
                dev.SetViewScissors(data.Size.Width, data.Size.Height);
                dev.DrawFullscreenTriangle(data.Size.Depth);
            }

            quad.Unbind();
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
        }

        public void Dispose()
        {
            shader?.Dispose();
            shader3D?.Dispose();
        }

        private static string GetSource(IShaderBuilder builder)
        {
            return $@"
{builder.SrvSingleType} src_image : register(t0);

cbuffer InputBuffer : register(b0) {{
    int3 size;
    bool hasAlpha;
}};

// texel helper function
#if {builder.Is3DInt}
uint3 texel(uint3 coord) {{ return coord; }}
#else
uint2 texel(uint3 coord) {{ return coord.xy; }}
#endif

struct PixelIn
{{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
#if {builder.Is3DInt}
    uint depth : SV_RenderTargetArrayIndex;
#endif
}};

float4 main(PixelIn pin) : SV_TARGET {{
    int3 id = int3(pin.projPos.xy, 0);
#if {builder.Is3DInt}
    id.z = pin.depth;
#endif

    int3 coord = 0;
    
    float4 dstColor = 0.0;
    int weightSum = 0.0;

    // apply filter
    for(coord.z = max(id.z - 1, 0); coord.z <= min(id.z + 1, size.z - 1); ++coord.z)
    for(coord.y = max(id.y - 1, 0); coord.y <= min(id.y + 1, size.y - 1); ++coord.y)
    for(coord.x = max(id.x - 1, 0); coord.x <= min(id.x + 1, size.x - 1); ++coord.x) {{
        float4 v = src_image[texel(coord)];
        int iw = 4 >> dot(abs(id - coord), 1);  // 4 >> 0 for center, 4 >> 1 == 2 for one difference, 4 >> 2 == 1 for two difference
        weightSum += iw;
        float w = float(iw);
        dstColor.a += v.a * w;
        dstColor.rgb += v.a * v.rgb * w;
    }}

    dstColor /= float(weightSum);
    if(!hasAlpha) dstColor.a = 1.0;
    if(dstColor.a != 0.0) dstColor.rgb /= dstColor.a;

    return dstColor;
}}
";
        }

        // unit testing purposes
        internal void CompileShaders()
        {
            DirectX.Shader s;
            s = Shader;
            s = Shader3D;
        }
    }
}
