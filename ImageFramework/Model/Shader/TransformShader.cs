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
    public class TransformShader : IDisposable
    {
        private DirectX.Shader shader;
        private DirectX.Shader shader3D;
        private readonly string pixelTypeIn;
        private readonly string pixelTypeOut;
        private readonly string transform;
        private readonly string[] inputs;

        public static readonly string TransformLuma = StatisticsShader.LumaValue;
        //public static readonly string TransformLuma = "return  dot(value.a * sign(value.rgb) * toSrgb(abs(value)).rgb, float3(0.299, 0.587, 0.114))";

        /// <summary>
        /// ctor for a single input image
        /// </summary>
        /// <param name="transform">transform "value" from pixelTypeIn to pixelTypeOut</param>
        /// <param name="pixelTypeIn"></param>
        /// <param name="pixelTypeOut"></param>
        public TransformShader(string transform, string pixelTypeIn, string pixelTypeOut)
        : this(
            new string[]{"src_image"}, 
            $"{pixelTypeIn} value = src_image[coord]; {transform}", 
            pixelTypeIn, pixelTypeOut)
        {}

        /// <summary>
        /// ctor for multiple input images
        /// </summary>
        /// <param name="inputs">names of input images</param>
        /// <param name="transform">use "coord" to fetch and tranform data of input images to pixelTypeOut</param>
        /// <param name="pixelTypeIn"></param>
        /// <param name="pixelTypeOut"></param>
        public TransformShader(string[] inputs, string transform, string pixelTypeIn, string pixelTypeOut)
        {
            this.inputs = inputs;
            this.transform = transform;
            this.pixelTypeIn = pixelTypeIn;
            this.pixelTypeOut = pixelTypeOut;
        }

        private DirectX.Shader Shader => shader ?? (shader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSource(new ShaderBuilder2D(pixelTypeIn), new ShaderBuilder2D(pixelTypeOut)), "GaussShader"));
        private DirectX.Shader Shader3D => shader3D ?? (shader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                               GetSource(new ShaderBuilder3D(pixelTypeIn), new ShaderBuilder3D(pixelTypeOut)), "GaussShader3D"));

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

        public void Run(ITexture src, ITexture dst, LayerMipmapSlice lm, UploadBuffer upload)
        {
            Run(new[]{src}, dst, lm, upload);
        }

        public void Run(ITexture[] sources, ITexture dst, LayerMipmapSlice lm, UploadBuffer upload)
        {
            Debug.Assert(sources.Length == inputs.Length);
            foreach (var src in sources)
            {
                Debug.Assert(src.HasSameDimensions(dst));
            }

            var size = sources[0].Size.GetMip(lm.SingleMipmap);
            upload.SetData(new BufferData
            {
                Layer = lm.SingleLayer,
                Size = size
            });
            var dev = Device.Get();
            var builder = sources[0].Is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;
            dev.Compute.Set(sources[0].Is3D ? Shader3D.Compute : Shader.Compute);
            dev.Compute.SetConstantBuffer(0, upload.Handle);
            for (var i = 0; i < sources.Length; i++)
            {
                var src = sources[i];
                dev.Compute.SetShaderResource(i, src.GetSrView(lm));
            }

            dev.Compute.SetUnorderedAccessView(0, dst.GetUaView(lm.SingleMipmap));

            dev.Dispatch(
                Utility.Utility.DivideRoundUp(size.X, builder.LocalSizeX),
                Utility.Utility.DivideRoundUp(size.Y, builder.LocalSizeY),
                Utility.Utility.DivideRoundUp(size.Z, builder.LocalSizeZ)
            );

            for (var i = 0; i < sources.Length; i++)
            {
                dev.Compute.SetShaderResource(i, null);
            }
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
{GetInputs(builderIn)}
{builderOut.UavType} out_image : register(u0);

cbuffer InputBuffer : register(b0) {{
    int layer;
    int3 size;
}};

{Utility.Utility.ToSrgbFunction()}
{builderIn.TexelHelperFunctions}

{builderOut.Type} transform({builderIn.IntVec} coord) {{
    {transform};
}}

[numthreads({builderIn.LocalSizeX}, {builderIn.LocalSizeY}, {builderIn.LocalSizeZ})]
void main(int3 id : SV_DispatchThreadID)
{{
    if(any(id >= size)) return;
    out_image[texel(id, layer)] = transform(texel(id));
}}
";
        }

        private string GetInputs(IShaderBuilder builder)
        {
            string res = "";
            int id = 0;
            foreach (var input in inputs)
            {
                res += $"{builder.SrvSingleType} {input} : register(t{id++});\n";
            }

            return res;
        }
    }
}
