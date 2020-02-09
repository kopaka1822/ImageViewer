using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;

namespace ImageFramework.Model.Shader
{
    /// <summary>
    /// transforms all values from an image
    /// </summary>
    class TransformShader : IDisposable
    {
        private DirectX.Shader shader;
        private DirectX.Shader shader3D;
        private readonly string pixelTypeIn;
        private readonly string pixelTypeOut;
        private readonly string transform;

        public static readonly string TransformLuma = StatisticsShader.LumaValue;

        public TransformShader(string transform, string pixelTypeIn = "float4", string pixelTypeOut = "float4")
        {
            this.transform = transform;
            this.pixelTypeIn = pixelTypeIn;
            this.pixelTypeOut = pixelTypeOut;
        }

        private DirectX.Shader Shader => shader ?? (shader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSource(new ShaderBuilder2D(pixelTypeIn), new ShaderBuilder2D(pixelTypeOut)), "GaussShader"));
        private DirectX.Shader Shader3D => shader3D ?? (shader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                               GetSource(new ShaderBuilder3D(pixelTypeIn), new ShaderBuilder2D(pixelTypeOut)), "GaussShader3D"));

        internal void CompileShaders()
        {
            var s = Shader;
            s = Shader3D;
        }

        private struct BufferData
        {
            public int Layer;
            public Size3 Size;
        }

        public void Run(ITexture src, ITexture dst, int layer, int mipmap, UploadBuffer upload)
        {
            Debug.Assert(src.HasSameDimensions(dst));

            var size = src.Size.GetMip(mipmap);
            upload.SetData(new BufferData
            {
                Layer = layer,
                Size = size
            });
            var dev = Device.Get();
            var builder = src.Is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;
            dev.Compute.Set(src.Is3D ? Shader3D.Compute : Shader.Compute);
            dev.Compute.SetConstantBuffer(0, upload.Handle);
            dev.Compute.SetShaderResource(0, src.GetSrView(layer, mipmap));
            dev.Compute.SetUnorderedAccessView(0, dst.GetUaView(mipmap));

            dev.Dispatch(
                Utility.Utility.DivideRoundUp(size.X, builder.LocalSizeX),
                Utility.Utility.DivideRoundUp(size.Y, builder.LocalSizeY),
                Utility.Utility.DivideRoundUp(size.Z, builder.LocalSizeZ)
            );

            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);
        }

        public void Dispose()
        {
            shader?.Dispose();
            shader3D?.Dispose();
        }

        private string GetSource(IShaderBuilder builderIn, IShaderBuilder builderOut)
        {
            return $@"
{builderIn.SrvSingleType} src_image : register(t0);
{builderOut.UavType} out_image : register(u0);

cbuffer InputBuffer : register(b0) {{
    int layer;
    int3 size;
}};

{Utility.Utility.ToSrgbFunction()}
{builderIn.TexelHelperFunctions}

{builderOut.Type} transform({builderIn.Type} value) {{
    {transform};
}}

[numthreads({builderIn.LocalSizeX}, {builderIn.LocalSizeY}, {builderIn.LocalSizeZ})]
void main(int3 id : SV_DispatchThreadID)
{{
    if(any(id >= size)) return;
    out_image[texel(id, layer)] = transform(src_image[texel(id)]);
}}
";
        }
    }
}
