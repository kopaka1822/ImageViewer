using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    internal class GaussShader : IDisposable
    {
        private readonly int radius;
        private readonly float variance;
        private DirectX.Shader shader;
        private DirectX.Shader shader3D;
        private readonly string pixelType;

        private DirectX.Shader Shader => shader ?? (shader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSource(new ShaderBuilder2D(pixelType)), "GaussShader"));
        private DirectX.Shader Shader3D => shader3D ?? (shader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSource(new ShaderBuilder3D(pixelType)), "GaussShader3D"));

        internal void CompileShaders()
        {
            var s = Shader;
            s = Shader3D;
        }

        public GaussShader(int radius, float variance, string pixelType = "float")
        {
            this.radius = radius;
            this.variance = variance;
            this.pixelType = pixelType;
        }

        private struct BufferData
        {
            public int Layer;
            public Size3 Direction;
            public Size3 Size;
        }

        public void Run(ITexture src, ITexture dst, LayerMipmapSlice lm, UploadBuffer buffer, ITextureCache cache)
        {
            Debug.Assert(src.HasSameDimensions(dst));
            Debug.Assert(cache.IsCompatibleWith(src));
            Debug.Assert(lm.IsIn(src.LayerMipmap));

            var srcSize = src.Size.GetMip(lm.Mipmap);

            var dev = Device.Get();
            dev.Compute.Set(src.Is3D ? Shader3D.Compute : Shader.Compute);
            var builder = src.Is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;

            // execute x
            buffer.SetData(new BufferData
            {
                Size = srcSize,   
                Direction = new Size3(1, 0, 0),
                Layer = lm.Layer
            });
            dev.Compute.SetConstantBuffer(0, buffer.Handle);
            dev.Compute.SetShaderResource(0, src.GetSrView(lm));
            var tmp1 = cache.GetTexture();
            ITexture tmp2 = null;
            dev.Compute.SetUnorderedAccessView(0, tmp1.GetUaView(lm.Mipmap));

            dev.Dispatch(
                Utility.Utility.DivideRoundUp(srcSize.Width, builder.LocalSizeX),
                Utility.Utility.DivideRoundUp(srcSize.Height, builder.LocalSizeY),
                Utility.Utility.DivideRoundUp(srcSize.Depth, builder.LocalSizeZ)
            );
       
            UnbindResources(dev);

            // execute y
            buffer.SetData(new BufferData
            {
                Size = srcSize,
                Direction = new Size3(0, 1, 0),
                Layer = lm.Layer
            });
            dev.Compute.SetConstantBuffer(0, buffer.Handle);
            dev.Compute.SetShaderResource(0, tmp1.GetSrView(lm));
            if (src.Is3D)
            {
                tmp2 = cache.GetTexture();
                dev.Compute.SetUnorderedAccessView(0, tmp2.GetUaView(lm.Mipmap));

                dev.Dispatch(
                    Utility.Utility.DivideRoundUp(srcSize.Width, builder.LocalSizeX),
                    Utility.Utility.DivideRoundUp(srcSize.Height, builder.LocalSizeY),
                    Utility.Utility.DivideRoundUp(srcSize.Depth, builder.LocalSizeZ)
                );

                UnbindResources(dev);

                // execute z
                buffer.SetData(new BufferData
                {
                    Size = srcSize,
                    Direction = new Size3(0, 0, 1),
                    Layer = lm.Layer
                });
                dev.Compute.SetConstantBuffer(0, buffer.Handle);
                dev.Compute.SetShaderResource(0, tmp2.GetSrView(lm));
            }

            // bind final target
            dev.Compute.SetUnorderedAccessView(0, dst.GetUaView(lm.Mipmap));

            dev.Dispatch(
                Utility.Utility.DivideRoundUp(srcSize.Width, builder.LocalSizeX),
                Utility.Utility.DivideRoundUp(srcSize.Height, builder.LocalSizeY),
                Utility.Utility.DivideRoundUp(srcSize.Depth, builder.LocalSizeZ)
            );

            UnbindResources(dev);
            cache.StoreTexture(tmp1);
            if(tmp2 != null) cache.StoreTexture(tmp2);
        }

        private void UnbindResources(Device dev)
        {
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);
        }

        public void Dispose()
        {
            shader?.Dispose();
            shader3D?.Dispose();
        }

        private string GetSource(IShaderBuilder builder)
        {
            return $@"
{builder.SrvSingleType} src_image : register(t0);
{builder.UavType} out_image : register(u0);

cbuffer InputBuffer : register(b0) {{
    int layer;
    int3 dir; // filter direction
    int3 size;
}};

{builder.TexelHelperFunctions}

float kernel(int offset) {{
    return exp(-0.5 * offset * offset / {variance});
}}

bool isInside(int3 pos) {{
    return all(pos < size) && all(pos >= 0);
}}

[numthreads({builder.LocalSizeX}, {builder.LocalSizeY}, {builder.LocalSizeZ})]
void main(int3 id : SV_DispatchThreadID)
{{
    if(any(id >= size)) return;

    float weightSum = 0.0;
    {builder.Type} pixelSum = 0.0;

    int3 pos = id - {radius} * dir;

    [unroll] for(int i = -{radius}; i <= {radius}; ++i) 
    {{
        [flatten] if(isInside(pos)){{
            float w = kernel(i);
            weightSum += w;
            pixelSum += w * src_image[texel(pos)];
        }}
        pos += dir;
    }}

    out_image[texel(id, layer)] = pixelSum / weightSum;
}}
";
        }
    }
}
