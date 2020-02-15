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

namespace ImageFramework.Model.Statistics
{
    public class MultiscaleSSIMShader : IDisposable
    {
        private DirectX.Shader wshader;
        private DirectX.Shader wshader3D;
        private DirectX.Shader cshader;
        private DirectX.Shader cshader3D;
        private readonly SamplerState sampler;


        private DirectX.Shader Shader => wshader ?? (wshader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSourceWeighted(new ShaderBuilder2D("float")), "MSSSIMShader"));
        private DirectX.Shader Shader3D => wshader3D ?? (wshader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                               GetSourceWeighted(new ShaderBuilder3D("float")), "MSSSIMShader3D"));

        private DirectX.Shader CopyShader => cshader ?? (cshader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSourceCopy(new ShaderBuilder2D("float")), "CopyMSSSIMShader"));
        private DirectX.Shader CopyShader3D => cshader3D ?? (cshader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                   GetSourceCopy(new ShaderBuilder3D("float")), "CopyMSSSIMShader3D"));

        public MultiscaleSSIMShader()
        {
            sampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = SharpDX.Direct3D11.Filter.MinMagMipLinear,
            });
        }

        internal void CompileShaders()
        {
            var s = Shader;
            s = Shader3D;
            s = CopyShader;
            s = CopyShader3D;
        }

        private struct BufferData
        {
            public int Layer;
            public Size3 Size;
            public float InvWeightSum;
            public float NumMipmaps;
        }

        /// <summary>
        /// copies the values of the +4th mipmap into the specified mipmap
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="lm">dst layer/mipmap</param>
        /// <param name="upload"></param>
        public void RunCopy(ITexture tex, LayerMipmapSlice lm, UploadBuffer upload)
        {
            var size = tex.Size.GetMip(lm.SingleMipmap);
            var nMipmaps = tex.NumMipmaps - lm.Mipmap;
            if (nMipmaps == 1) return; // already finished

            upload.SetData(new BufferData
            {
                Layer = lm.SingleLayer,
                Size = size
            });
            var dev = Device.Get();

            var builder = tex.Is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;
            dev.Compute.Set(tex.Is3D ? CopyShader3D.Compute : CopyShader.Compute);
            dev.Compute.SetConstantBuffer(0, upload.Handle);
            dev.Compute.SetSampler(0, sampler);

            dev.Compute.SetUnorderedAccessView(0, tex.GetUaView(0));
            dev.Compute.SetShaderResource(0, tex.GetSrView(lm.AddMipmap(Math.Min(4, nMipmaps - 1))));
            
            dev.Dispatch(
                Utility.Utility.DivideRoundUp(size.X, builder.LocalSizeX),
                Utility.Utility.DivideRoundUp(size.Y, builder.LocalSizeY),
                Utility.Utility.DivideRoundUp(size.Z, builder.LocalSizeZ)
            );

            dev.Compute.SetShaderResource(0, null);            
            dev.Compute.SetUnorderedAccessView(0, null);
        }

        /// <summary>
        /// weights the values of the given mipmap with values from higher mipmaps (according to ms-ssim)
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="lm">dst layer/mipmap</param>
        /// <param name="upload"></param>
        public void RunWeighted(ITexture tex, LayerMipmapSlice lm, UploadBuffer upload)
        {
            var size = tex.Size.GetMip(lm.SingleMipmap);
            var nMipmaps = tex.NumMipmaps - lm.Mipmap;
            if (nMipmaps == 1) return; // already finished

            upload.SetData(new BufferData
            {
                Layer = lm.SingleLayer,
                Size = size,
                InvWeightSum = GetInvWeightSum(nMipmaps),
                NumMipmaps = nMipmaps
            });
            var dev = Device.Get();

            var builder = tex.Is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;
            dev.Compute.Set(tex.Is3D ? Shader3D.Compute : Shader.Compute);
            dev.Compute.SetConstantBuffer(0, upload.Handle);
            dev.Compute.SetSampler(0, sampler);

            dev.Compute.SetUnorderedAccessView(0, tex.GetUaView(lm.Mipmap));
            dev.Compute.SetShaderResource(0, nMipmaps >= 2 ? tex.GetSrView(lm.AddMipmap(1)) : null);
            dev.Compute.SetShaderResource(1, nMipmaps >= 3 ? tex.GetSrView(lm.AddMipmap(2)) : null);
            dev.Compute.SetShaderResource(2, nMipmaps >= 4 ? tex.GetSrView(lm.AddMipmap(3)) : null);
            dev.Compute.SetShaderResource(3, nMipmaps >= 5 ? tex.GetSrView(lm.AddMipmap(4)) : null);
            
            dev.Dispatch(
                Utility.Utility.DivideRoundUp(size.X, builder.LocalSizeX),
                Utility.Utility.DivideRoundUp(size.Y, builder.LocalSizeY),
                Utility.Utility.DivideRoundUp(size.Z, builder.LocalSizeZ)
            );

            for (var i = 0; i < 4; i++)
            {
                dev.Compute.SetShaderResource(i, null);
            }
            dev.Compute.SetUnorderedAccessView(0, null);
        }

        public void Dispose()
        {
            wshader?.Dispose();
            wshader3D?.Dispose();
            sampler?.Dispose();
        }

        private string GetSourceWeighted(IShaderBuilder builder)
        {
            return $@"
{builder.UavType} primary_layer : register(u0);
{builder.SrvSingleType} other_layer[4] : register(t0);
SamplerState texSampler : register(s0);

cbuffer InputBuffer : register(b0) {{
    int layer;
    int3 size;
    float invWeightSum; // 1.0 if all mipmaps are used
    int numMipmaps;
}};

{builder.TexelHelperFunctions}

[numthreads({builder.LocalSizeX}, {builder.LocalSizeY}, {builder.LocalSizeZ})]
void main(int3 id : SV_DispatchThreadID)
{{
    if(any(id >= size)) return;
    float color = 1.0;
    // keep sign of upper layer (important for structure)
    float sign = primary_layer[texel(id, layer)] < 0 ? -1.0f : 1.0f;
    color *= pow(abs(primary_layer[texel(id, layer)]), 0.0448 * invWeightSum);
    float3 tId = (id + 0.5) / size;
    
    color *= pow(abs(other_layer[0].SampleLevel(texSampler, texel(tId), 0)), 0.2856 * invWeightSum);
    if(numMipmaps >= 3)
        color *= pow(abs(other_layer[1].SampleLevel(texSampler, texel(tId), 0)), 0.3001 * invWeightSum);
    if(numMipmaps >= 4)
        color *= pow(abs(other_layer[2].SampleLevel(texSampler, texel(tId), 0)), 0.2363 * invWeightSum);
    if(numMipmaps >= 5)
        color *= pow(abs(other_layer[3].SampleLevel(texSampler, texel(tId), 0)), 0.1333 * invWeightSum);

    primary_layer[texel(id, layer)] = sign * color;
}}
";
        }

        private string GetSourceCopy(IShaderBuilder builder)
        {
            return $@"
{builder.UavType} dst_layer : register(u0);
{builder.SrvSingleType} src_layer : register(t0);
SamplerState texSampler : register(s0);

cbuffer InputBuffer : register(b0) {{
    int layer;
    int3 size;
}};

{builder.TexelHelperFunctions}

[numthreads({builder.LocalSizeX}, {builder.LocalSizeY}, {builder.LocalSizeZ})]
void main(int3 id : SV_DispatchThreadID)
{{
    if(any(id >= size)) return;
    
    float3 tId = (id + 0.5) / size;
    dst_layer[texel(id, layer)] = src_layer.SampleLevel(texSampler, texel(tId), 0);
}}
";
        }


        private float GetInvWeightSum(int nMipmaps)
        {
            switch (nMipmaps)
            {
                case 1: return 1.0f / 0.0448f;
                case 2: return 1.0f / 0.3304f;
                case 3: return 1.0f / 0.6305f;
                case 4: return 1.0f / 0.8668f;
                default:
                    return 1.0f;
            }
        }
    }
}
