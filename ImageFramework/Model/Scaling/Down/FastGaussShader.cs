using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model.Scaling.Down
{
    // fast gauss shader for detail preserving downscaling
    internal class FastGaussShader : IDisposable
    {
        private DirectX.Shader shader = null;
        private DirectX.Shader shader3D = null;

        private DirectX.Shader Shader => shader ?? (shader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSource(ShaderBuilder.Builder2D), "FastGauss"));

        private DirectX.Shader Shader3D => shader3D ?? (shader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSource(ShaderBuilder.Builder3D), "FastGauss3D"));

        private struct BufferData
        {
            public Size3 Size;
            public int Layer;
            public int HasAlpha;
        }

        // executes a 3x3 gauss kernel on the given texture mipmap
        public void Run(ITexture texSrc, ITexture texDst, int mipmap, bool hasAlpha, UploadBuffer upload)
        {
            Debug.Assert(texSrc.NumLayers == texDst.NumLayers);
            Debug.Assert(texSrc.NumMipmaps == texDst.NumMipmaps);
            Debug.Assert(texSrc.Size == texDst.Size);

            var dev = Device.Get();
            dev.Compute.Set(texSrc.Is3D ? Shader3D.Compute : Shader.Compute);
            var builder = texSrc.Is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;

            var data = new BufferData
            {
                Size = texSrc.Size.GetMip(mipmap),
                HasAlpha = hasAlpha?1:0,
            };

            for (int layer = 0; layer < texSrc.NumLayers; ++layer)
            {
                data.Layer = layer;
                upload.SetData(data);
                dev.Compute.SetConstantBuffer(0, upload.Handle);
                dev.Compute.SetShaderResource(0, texSrc.GetSrView(layer, mipmap));
                dev.Compute.SetUnorderedAccessView(0, texDst.GetUaView(mipmap));

                dev.Dispatch(
                    Utility.Utility.DivideRoundUp(data.Size.Width, builder.LocalSizeX),
                    Utility.Utility.DivideRoundUp(data.Size.Height, builder.LocalSizeY),
                    Utility.Utility.DivideRoundUp(data.Size.Depth, builder.LocalSizeZ)
                );
            }

            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);
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
{builder.UavType} dst_image : register(u0);

cbuffer InputBuffer : register(b0) {{
    int3 size;
    uint layer;
    bool hasAlpha;
}};

// texel helper function
#if {builder.Is3DInt}
uint3 texel(uint3 coord) {{ return coord; }}
uint3 texel(uint3 coord, uint layer) {{ return coord; }}
#else
uint2 texel(uint3 coord) {{ return coord.xy; }}
uint3 texel(uint3 coord, uint layer) {{ return uint3(coord.xy, layer); }}
#endif

[numthreads({builder.LocalSizeX}, {builder.LocalSizeY}, {builder.LocalSizeZ})]
void main(int3 id : SV_DispatchThreadID) {{
    if(any(id >= size)) return;

    int3 coord = 0;
    
    float4 dstColor = 0.0;
    float weightSum = 0.0;

    // apply filter
    for(coord.z = max(id.z - 1, 0); coord.z <= max(id.z + 1, size.z - 1); ++coord.z)
    for(coord.y = max(id.y - 1, 0); coord.y <= max(id.y + 1, size.y - 1); ++coord.y)
    for(coord.x = max(id.z - 1, 0); coord.x <= max(id.x + 1, size.x - 1); ++coord.x) {{
        float4 v = src_image[texel(coord)];
        float w = float(4 >> dot(abs(id - coord), 1)); // 4 >> 0 for center, 4 >> 1 == 2 for one difference, 4 >> 2 == 1 for two difference
        weightSum += w;
        dstColor.a += v.a * w;
        dstColor.rgb += v.a * v.rgb * w;
    }}

    dstColor /= weightSum;
    if(!hasAlpha) dstColor.a = 1.0;
    if(dstColor.a != 0.0) dstColor.rgb /= dstColor.a;

    dst_image[texel(coord, layer)] = dstColor;
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
