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
    /// shader with a non seperatable kernel
    /// </summary>
    internal abstract class NonSepKernelShader : IDisposable
    {
        private readonly int radius;
        private DirectX.Shader shader;
        private DirectX.Shader shader3D;
        private readonly string pixelType;
        private readonly string[] inputs;
        private readonly string beforeLoop;
        private readonly string inLoop;
        private readonly string afterLoop;

        private static readonly int LocalSizeDivide = 2;

        private DirectX.Shader Shader => shader ?? (shader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                             GetSource(new ShaderBuilder2D(pixelType)), "NonSepKernel"));
        private DirectX.Shader Shader3D => shader3D ?? (shader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                               GetSource(new ShaderBuilder3D(pixelType)), "NonSepKernel3D"));

        internal void CompileShaders()
        {
            var s = Shader;
            s = Shader3D;
        }
        protected NonSepKernelShader(int radius, string[] inputs, string beforeLoop, string inLoop, string afterLoop, string pixelType = "float")
        {
            this.radius = radius;
            this.inputs = inputs;
            this.beforeLoop = beforeLoop;
            this.inLoop = inLoop;
            this.afterLoop = afterLoop;
            this.pixelType = pixelType;
        }

        private struct BufferData
        {
            public int Layer;
            public Size3 Size;
        }

        protected void Run(ITexture[] sources, ITexture dst, LayerMipmapSlice lm, UploadBuffer upload)
        {
            Debug.Assert(sources.Length == inputs.Length);
            foreach (var src in sources)
            {
                Debug.Assert(src.HasSameDimensions(dst));
            }

            var size = dst.Size;
            upload.SetData(new BufferData
            {
                Layer = lm.Layer,
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
                Utility.Utility.DivideRoundUp(size.X, builder.LocalSizeX / LocalSizeDivide),
                Utility.Utility.DivideRoundUp(size.Y, builder.LocalSizeY / LocalSizeDivide),
                Utility.Utility.DivideRoundUp(size.Z, Math.Max(builder.LocalSizeZ / LocalSizeDivide, 1))
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

        private string GetSource(IShaderBuilder builder)
        {
            return $@"
{GetInputs(builder)}
{builder.UavType} out_image : register(u0);

cbuffer InputBuffer : register(b0) {{
    int layer;
    int3 size;
}};

bool isInside(int3 pos) {{
    return all(pos < size) && all(pos >= 0);
}}

{Utility.Utility.ToSrgbFunction()}
{builder.TexelHelperFunctions}

[numthreads({builder.LocalSizeX / LocalSizeDivide}, {builder.LocalSizeY / LocalSizeDivide}, {Math.Max(builder.LocalSizeZ / LocalSizeDivide, 1)})]
void main(int3 id : SV_DispatchThreadID)
{{
    if(any(id >= size)) return;
    
    int3 start = id - {radius};
    int3 end = id + {radius};
#if {1 - builder.Is3DInt} // if not 3d
    start.z = 0;
    end.z = 0;
#endif
    
    {beforeLoop}

    int3 coord = start;
#if {builder.Is3DInt}
    for(coord.z = start.z; coord.z <= end.z; ++coord.z)
#endif
    for(coord.y = start.y; coord.y <= end.y; ++coord.y)
    [unroll] for(coord.x = start.x; coord.x <= end.x; ++coord.x)
    {{
        [flatten] if(isInside(coord)) {{
            {inLoop}
        }}
    }}

    {afterLoop}
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
