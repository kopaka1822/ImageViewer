
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Model.Filter;
using ImageFramework.Model.Filter.Parameter;

namespace ImageFramework.Model.Shader
{
    internal class FilterShader : IDisposable
    {
        private readonly FilterModel parent;
        private readonly DirectX.Shader shader;
        private readonly UploadBuffer<int> paramBuffer;

        public readonly int localSize;

        public FilterShader(FilterModel parent, string source, int groupSize)
        {
            localSize = groupSize;
            this.parent = parent;
            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetShaderHeader() + "\n#line 1\n" + source, parent.Filename);
            if (parent.Parameters.Count != 0)
            {
                paramBuffer = new UploadBuffer<int>(parent.Parameters.Count);
                UpdateParamBuffer();
            }
        }

        /// <summary>
        /// executes on iteration of the filter (for all layers and mipmaps)
        /// </summary>
        /// <param name="image">original images (might be used for texture bindings)</param>
        /// <param name="src">source texture</param>
        /// <param name="dst">destination texture</param>
        /// <param name="cbuffer">buffer that stores some runtime information</param>
        /// <param name="iteration">current filter iteration. Should be 0 if not separable. Should be 0 or 1 if separable (x- and y-direction pass)</param>
        /// <remarks>make sure to call UpdateParamBuffer() if parameters have changed after the last invocation</remarks>
        internal void Run(ImagesModel image, TextureArray2D src, TextureArray2D dst, UploadBuffer<LayerLevelFilter> cbuffer, int iteration)
        {
            if (parent.IsSepa) Debug.Assert(iteration == 0 || iteration == 1);
            else Debug.Assert(iteration == 0);

            var dev = Device.Get();
            dev.Compute.Set(shader.Compute);

            // filter parameters (constant)
            if(paramBuffer != null)
                dev.Compute.SetConstantBuffer(1, paramBuffer.Handle);

            for (int curMipmap = 0; curMipmap < image.NumMipmaps; ++curMipmap)
            {
                // dst texture
                dev.Compute.SetUnorderedAccessView(0, dst.GetUaView(curMipmap));
                var width = image.GetWidth(curMipmap);
                var height = image.GetHeight(curMipmap);

                for (int curLayer = 0; curLayer < image.NumLayers; ++curLayer)
                {
                    // src textures
                    dev.Compute.SetShaderResource(0, src.GetSrView(curLayer, curMipmap));
                    BindTextureParameters(image, curLayer, curMipmap);

                    cbuffer.SetData(new LayerLevelFilter
                    {
                        Layer = curLayer,
                        Level = curMipmap,
                        FilterX = iteration,
                        FilterY = 1 - iteration
                    });

                    dev.Compute.SetConstantBuffer(0, cbuffer.Handle);

                    dev.Dispatch(Utility.Utility.DivideRoundUp(width, localSize), Utility.Utility.DivideRoundUp(height, localSize));
                }
            }

            // remove texture bindings
            dev.Compute.SetUnorderedAccessView(0, null);
            dev.Compute.SetShaderResource(0, null);
        }

        /// <summary>
        /// reloads all values into the buffer
        /// </summary>
        internal void UpdateParamBuffer()
        {
            var data = new int[parent.Parameters.Count];
            for (var i = 0; i < data.Length; ++i)
            {
                data[i] = parent.Parameters[i].StuffToInt();
            }

            paramBuffer.SetData(data);
        }

        private void BindTextureParameters(ImagesModel image, int layer, int mipmap)
        {
            var texSlot = 1;
            foreach (var texParam in parent.TextureParameters)
            {
                Device.Get().Compute.SetShaderResource(texSlot++, image.Images[texParam.Source].Image.GetSrView(layer, mipmap));
            }
        }

        private string GetShaderHeader()
        {
            string filterDirectionVar = parent.IsSepa ? "int2 filterDirection;" : "";

            return $@"
RWTexture2DArray<float4> dst_image : register(u0);
Texture2D<float4> src_image : register(t0);

cbuffer LayerLevelBuffer : register(b0) {{
    uint layer;
    uint level;
    {filterDirectionVar}
}};

float4 filter(int2 pixel, int2 size);

[numthreads({localSize}, {localSize}, 1)]
void main(uint3 coord : SV_DISPATCHTHREADID) {{
    
    uint width, height, elements;
    dst_image.GetDimensions(width, height, elements);
    if(coord.x >= width || coord.y >= height) return;    

    dst_image[uint3(coord.x, coord.y, layer)] = filter(int2(coord.xy), int2(width, height));
}}
" + GetParamBufferDescription(parent.Parameters) + GetTextureParamBindings(parent.TextureParameters);
        }

        private static string GetParamBufferDescription(IReadOnlyList<IFilterParameter> parameters)
        {
            if (parameters.Count == 0) return "";
            string res = "cbuffer FilterParamBuffer : register(b1) {\n";
            foreach (var filterParameter in parameters)
            {
                res += "   " + filterParameter.GetParamterType().ToString().ToLower() + " " +
                       filterParameter.GetBase().VariableName + ";\n";
            }

            return res + "};\n";
        }

        private static string GetTextureParamBindings(IReadOnlyList<TextureFilterParameterModel> parameters)
        {
            if (parameters.Count == 0) return "";

            string res = "";

            var i = 1;
            foreach (var tex in parameters)
            {
                res += "Texture2D<float4> " + tex.TextureName + $" : register(t{i++});\n";
            }

            return res;
        }

        public void Dispose()
        {
            shader?.Dispose();
            paramBuffer?.Dispose();
        }
    }
}
