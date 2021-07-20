
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Model.Filter;
using ImageFramework.Model.Filter.Parameter;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    internal class FilterShader : IDisposable
    {
        private static readonly int TextureBindingStart = 2;
        private readonly FilterModel parent;
        private readonly DirectX.Shader shader;
        private readonly UploadBuffer paramBuffer;
        private readonly GpuBuffer continueIterationBuffer;
        private readonly SharedModel shared;

        private readonly int localSize;
        private readonly bool is3D;

        public FilterShader(FilterModel parent, string source, int groupSize, FilterLoader.Type kernel, IShaderBuilder builder, SharedModel shared)
        {
            localSize = groupSize;
            this.parent = parent;
            this.shared = shared;
            is3D = builder.Is3D;
            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetShaderHeader(kernel, builder) + "\n#line 1\n" + source, parent.Filename);
            if (parent.Parameters.Count != 0)
            {
                paramBuffer = new UploadBuffer(4 * parent.Parameters.Count);
                UpdateParamBuffer();
            }
            if(parent.DoIterations) continueIterationBuffer = new GpuBuffer(4, 1);
        }

        /// <summary>
        /// Indicates if the iterations should be stopped (based on results from the previous Run())
        /// may only be called after Run() was executed (otherwise no meaningful value might be returned)
        /// </summary>
        internal bool AbortIterations
        {
            get
            {
                if (continueIterationBuffer == null) return false;

                // download buffer from gpu (should happen very fast since the shader was already executed when called after DoFilterIterationAsync)
                shared.Download.CopyFrom(continueIterationBuffer, 4);
                var continueIterations = shared.Download.GetData<int>() != 0;
                return !continueIterations;
            }
        }

        /// <summary>
        /// executes on iteration of the filter (for all layers and mipmaps)
        /// </summary>
        /// <param name="image">original images (might be used for texture bindings)</param>
        /// <param name="src">source texture</param>
        /// <param name="dst">destination texture</param>
        /// <param name="iteration">current filter iteration. Should be 0 if not separable. Should be 0 or 1 if separable (x- and y-direction pass) or 2 for z-direction pass</param>
        /// <param name="numMipmaps">number of mipmaps to apply the filter on (starting with most detailed mip)</param>
        /// <remarks>make sure to call UpdateParamBuffer() if parameters have changed after the last invocation</remarks>
        internal void Run(ImagesModel image, ITexture src, ITexture dst, int iteration, int numMipmaps)
        {
            if (parent.IsSepa) Debug.Assert(iteration == 0 || iteration == 1 || iteration == 2);
            else if (!parent.DoIterations) Debug.Assert(iteration == 0);

            // compatible textures?
            Debug.Assert(src.Is3D == is3D);
            Debug.Assert(dst.Is3D == is3D);

            var dev = Device.Get();
            dev.Compute.Set(shader.Compute);

            // filter parameters (constant)
            if(paramBuffer != null)
                dev.Compute.SetConstantBuffer(1, paramBuffer.Handle);

            dev.Compute.SetShaderResource(1, src.View);
            dev.Compute.SetSampler(0, shared.LinearSampler);
            dev.Compute.SetSampler(1, shared.PointSampler);
            if (parent.DoIterations)
            {
                // write zero (false) into the _g_continue_iterations_buffer to indicate
                // that iterations should be stopped (if no threads want to continue)
                shared.Upload.SetData(0);
                continueIterationBuffer.CopyFrom(shared.Upload); // copy zero to buffer
                dev.Compute.SetUnorderedAccessView(1, continueIterationBuffer.View);
            }

            for (int curMipmap = 0; curMipmap < numMipmaps; ++curMipmap)
            {
                // dst texture
                dev.Compute.SetUnorderedAccessView(0, dst.GetUaView(curMipmap));
                var size = image.GetSize(curMipmap);

                for (int curLayer = 0; curLayer < image.NumLayers; ++curLayer)
                {
                    // src textures
                    dev.Compute.SetShaderResource(0, src.GetSrView(new LayerMipmapSlice(curLayer, curMipmap)));
                    BindTextureParameters(image, new LayerMipmapSlice(curLayer, curMipmap));

                    shared.Upload.SetData(new LayerLevelFilter
                    {
                        Layer = curLayer,
                        Level = curMipmap,
                        Iteration = iteration,
                        FilterX = iteration == 0?1:0,
                        FilterY = iteration == 1?1:0,
                        FilterZ = iteration == 2?1:0
                    });

                    dev.Compute.SetConstantBuffer(0, shared.Upload.Handle);

                    dev.Dispatch(
                        Utility.Utility.DivideRoundUp(size.Width, localSize), 
                        Utility.Utility.DivideRoundUp(size.Height, localSize),
                        Utility.Utility.DivideRoundUp(size.Depth, localSize));
                }
            }

            // remove texture bindings
            dev.Compute.SetUnorderedAccessView(0, null);
            dev.Compute.SetUnorderedAccessView(1, null);
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetShaderResource(1, null);
            UnbindTextureParameters();
        }

        /// <summary>
        /// reloads all values into the buffer
        /// </summary>
        internal void UpdateParamBuffer()
        {
            if (paramBuffer == null) return;

            var data = new int[parent.Parameters.Count];
            for (var i = 0; i < data.Length; ++i)
            {
                data[i] = parent.Parameters[i].StuffToInt();
            }

            paramBuffer.SetData(data);
        }

        private void BindTextureParameters(ImagesModel image, LayerMipmapSlice lm)
        {
            var texSlot = TextureBindingStart;
            foreach (var texParam in parent.TextureParameters)
            {
                Device.Get().Compute.SetShaderResource(texSlot++, image.Images[texParam.Source].Image.GetSrView(lm));
            }
        }

        private void UnbindTextureParameters()
        {
            var texSlot = TextureBindingStart;
            foreach (var texParam in parent.TextureParameters)
            {
                Device.Get().Compute.SetShaderResource(texSlot++, null);
            }
        }

        private string GetShaderHeader(FilterLoader.Type kernel, IShaderBuilder builder)
        {
            string filterDirectionVar = parent.IsSepa ? "int3 filterDirection;" : "";

            return $@"
{builder.UavType}  dst_image : register(u0);
{builder.SrvSingleType} src_image : register(t0);
{builder.SrvType} src_image_ex : register(t1);

SamplerState linearSampler : register(s0);
SamplerState pointSampler : register(s1);

cbuffer LayerLevelBuffer : register(b0) {{
    uint layer;
    uint level;
    int iteration; // current iteration
    uint _padding_2;
    {filterDirectionVar}
}};

// texel helper function
#if {builder.Is3DInt}
int3 texel(int3 coord) {{ return coord; }}
int3 texel(int3 coord, int layer) {{ return coord; }}
#else
int2 texel(int3 coord) {{ return coord.xy; }}
int3 texel(int3 coord, int layer) {{ return int3(coord.xy, layer); }}
#endif

#if {(parent.DoIterations ? 1 : 0)}
// contains one bool value that was initialized to false
RWStructuredBuffer<bool> _g_continue_iterations_buffer : register(u1);

// continue iterations until ALL threads call abort_iteration()
static bool _g_continue_iterations = true;
void abort_iterations() {{
    _g_continue_iterations = false;
}}
#endif

float4 filter{GetKernelDeclaration(kernel)};

[numthreads({localSize}, {localSize}, {(builder.Is3D?localSize:1)})]
void main(uint3 coord : SV_DISPATCHTHREADID) {{
    
    uint width, height, depth;
    dst_image.GetDimensions(width, height, depth);
    uint3 dstCoord = coord;
    uint3 size = int3(width, height, depth);
    if(coord.x >= width || coord.y >= height) return;
#if {builder.Is3DInt}
    if(coord.z >= depth) return;
#else
    dstCoord.z = layer;
    size.z = 1;
#endif

#if {(kernel == FilterLoader.Type.Tex2D?1:0)}
     dst_image[dstCoord] = filter(coord.xy, size.xy);
#elif {(kernel == FilterLoader.Type.Color ? 1 : 0)}
    dst_image[dstCoord] = filter(src_image[texel(coord)]);
#else // dynamic or 3D
    dst_image[dstCoord] = filter(coord, size);
#endif

#if {(parent.DoIterations ? 1 : 0)}
    // test if at least one thread wants to continue
    if(_g_continue_iterations) // write true to global buffer variable (initial value is false)
        _g_continue_iterations_buffer[0] = true;
#endif
}}
" + GetParamBufferDescription(parent.Parameters) + GetTextureParamBindings(parent.TextureParameters, builder);
        }

        private static string GetKernelDeclaration(FilterLoader.Type kernel)
        {
            switch (kernel)
            {
                case FilterLoader.Type.Tex2D:
                    return "(int2 pixel, int2 size)";
                case FilterLoader.Type.Dynamic:
                case FilterLoader.Type.Tex3D:
                    return "(int3 pixel, int3 size)";
                case FilterLoader.Type.Color:
                    return "(float4 color)";
            }

            return "";
        }

        private static string GetParamBufferDescription(IReadOnlyList<IFilterParameter> parameters)
        {
            if (parameters.Count == 0) return "";
            string res = "cbuffer FilterParamBuffer : register(b1) {\n";
            foreach (var filterParameter in parameters)
            {
                res += "   " + filterParameter.GetShaderParameterType() + " " +
                       filterParameter.GetBase().VariableName + ";\n";
            }

            return res + "};\n";
        }

        private static string GetTextureParamBindings(IReadOnlyList<TextureFilterParameterModel> parameters, IShaderBuilder builder)
        {
            if (parameters.Count == 0) return "";

            string res = "";

            var i = TextureBindingStart;
            foreach (var tex in parameters)
            {
                res += $"{builder.SrvSingleType} {tex.TextureName} : register(t{i++});\n";
            }

            return res;
        }

        public void Dispose()
        {
            shader?.Dispose();
            paramBuffer?.Dispose();
            continueIterationBuffer?.Dispose();
        }
    }
}
