
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Model.Statistics;
using ImageFramework.Utility;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Scaling.AlphaTest
{
    internal class AlphaPyramidPostproces : PostprocessBase
    {
        private readonly StatisticsModel stats;
        private DirectX.Shader shader;
        private DirectX.Shader shader3D;

        public AlphaPyramidPostproces(StatisticsModel stats)
        {
            this.stats = stats;
            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetComputeSource(new ShaderBuilder2D()), "AlphaDistribution");
            shader3D = new DirectX.Shader(DirectX.Shader.Type.Compute, GetComputeSource(new ShaderBuilder3D()), "AlphaDistribution3D");
        }

        public override void Dispose()
        {
            shader?.Dispose();
            shader3D?.Dispose();

            base.Dispose();
        }

        private struct BufferData
        {
            public int Layer;
            public Size3 Size;
        }

        public override void Run(ITexture uav, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            var dev = Device.Get();

            for (int layer = 0; layer < uav.NumLayers; ++layer)
            {
                // obtain the desired coverage when alpha blending is used
                float desiredCoverage = stats.GetStatisticsFor(uav, new LayerMipmapSlice(layer, 0)).Alpha.Avg;

                // fix alpha values for all mipmaps (including the most detailed)
                for (int mip = 0; mip < uav.NumMipmaps; ++mip)
                {
                    var lm = new LayerMipmapSlice(layer, mip);
                    var size = uav.Size.GetMip(mip);

                    // determine number of visible pixels
                    int nOpaque = (int)Math.Ceiling(desiredCoverage * uav.Size.GetMip(mip).Product);

                    // obtain all alpha values
                    var alphas = uav.GetPixelAlphas(lm);
                    Debug.Assert(alphas.Length == size.Product);
                    //ErrorDiffusion(alphas, size.Width, size.Height, size.Depth, 1);

                    var gpuBuffer = new UploadBuffer(alphas.Length, BindFlags.ShaderResource,
                        ResourceOptionFlags.BufferAllowRawViews);
                    var gpuBufferSrv = new ShaderResourceView(dev.Handle, gpuBuffer.Handle,
                        new ShaderResourceViewDescription
                        {
                            Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                            Format = Format.R32_Typeless, // required for the raw view (ByteAddressBuffer)
                            BufferEx = new ShaderResourceViewDescription.ExtendedBufferResource
                            {
                                FirstElement = 0,
                                ElementCount = gpuBuffer.ByteSize / 4,
                                Flags = ShaderResourceViewExtendedBufferFlags.Raw
                            }
                        });
                    // upload alpha values
                    gpuBuffer.SetData(alphas);

                    // execute shader to update texture alpha values
                    var builder = uav.Is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;
                    dev.Compute.Set(uav.Is3D ? shader3D.Compute : shader.Compute);
                    upload.SetData(new BufferData
                    {
                        Layer = layer,
                        Size = size
                    });
                    dev.Compute.SetConstantBuffer(0, upload.Handle);
                    dev.Compute.SetUnorderedAccessView(0, uav.GetUaView(mip));
                    dev.Compute.SetShaderResource(0, gpuBufferSrv);
                    dev.Dispatch(
                        Utility.Utility.DivideRoundUp(size.X, builder.LocalSizeX),
                        Utility.Utility.DivideRoundUp(size.Y, builder.LocalSizeY),
                        Utility.Utility.DivideRoundUp(size.Z, builder.LocalSizeZ)
                    );
                    dev.Compute.SetShaderResource(0, null);
                    dev.Compute.SetUnorderedAccessView(0, null);
                    dev.Compute.SetConstantBuffer(0, null);

                    gpuBufferSrv.Dispose();
                    gpuBuffer.Dispose();
                }
            }
        }

        // adapted from: https://github.com/cemyuksel/cyCodeBase/blob/master/cyAlphaDistribution.h
        private void ErrorDiffusion(byte[] image, int width, int height, int depth, int spp)
        {
            Action<int, int> addError = (index, error) =>
            {
                int a = image[index] + error;
                if (a < 0) a = 0;
                if (a > 255) a = 255;
                image[index] = (byte)a;
            };

            if (width * height * depth == 1)
            {
                // fix for single pixel
                image[0] = 255;
                return; 
            }

            int i = 0; // index of current texel
            for (int id = 0; id < depth; ++id) // TODO properly adapt for 3D images
            {
                for (int ih = 0; ih < height; ++ih)
                {
                    for (int iw = 0; iw < width; ++iw, ++i)
                    {
                        int a0 = image[i];
                        int a1 = a0 >= 128 ? 255 : 0;
                        // multisample adjustment
                        if (spp > 1)
                        {
                            for (int j = 1; j <= spp; j++)
                            {
                                int cutoff = (256 * (j * 2 - 1)) / (spp * 2);
                                if (a0 < cutoff) break;
                                a1 = (256 * j) / spp;
                            }
                            if (a1 > 255) a1 = 255;
                        }
                        //
                        image[i] = (byte)a1;
                        // calculate 4 error values to distribute (e0-e3)
                        int err = a0 - a1;
                        int e0 = (7 * err) / 16;
                        int e1 = (3 * err) / 16;
                        int e2 = (5 * err) / 16;
                        int e3 = (1 * err) / 16;
                        int de = err - (e0 + e1 + e2 + e3);
                        e0 += de; // distribute remainder
                        // distribute e0 to (x+1,y  )
                        if (iw < width - 1) addError(i + 1, e0);
                        if (ih < height - 1)
                        {
                            // distribute e1 to (x-1,y+1)
                            if (iw > 0) addError(width + i - 1, e1);
                            // distribute e2 to (x  ,y+1)
                            addError(width + i, e2);
                            // distribute e3 to (x+1,y+1)
                            if(iw < width - 1) addError(width + i + 1, e3);
                        }
                    }
                }
            }
        }

        private string GetComputeSource(IShaderBuilder b)
        {
            return $@"
{b.UavType} image : register(u0);
ByteAddressBuffer alphas : register(t0); 

cbuffer InputBuffer : register(b0) {{
    int layer;
    int3 size;
}};

{b.TexelHelperFunctions}

[numthreads({b.LocalSizeX}, {b.LocalSizeY}, {b.LocalSizeZ})]
void main(int3 id : SV_DispatchThreadID)
{{
    if(any(id >= size)) return;
    int index = id.x + size.x * id.y + size.x * size.y * id.z;
    float alpha = (float)(alphas.Load(index) & 0xFF) / 255.0f;
    float4 value = image[texel(id, layer)];
    value.a = alpha;
    image[texel(id, layer)] = value;
}}
";
        }
    }
}
