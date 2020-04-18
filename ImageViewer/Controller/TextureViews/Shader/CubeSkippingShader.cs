using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class CubeSkippingShader : IDisposable
    {
        private readonly ImageFramework.DirectX.Shader compute;
        private TransformShader initTexShader;
        private readonly Size3 workgroupSize;

        public CubeSkippingShader()
        {
            workgroupSize = new Size3(
                ShaderBuilder.Builder3D.LocalSizeX, 
                ShaderBuilder.Builder3D.LocalSizeY,
                ShaderBuilder.Builder3D.LocalSizeZ
            );

            initTexShader = new TransformShader(
                "return value.a > 0 ? 0 : 255", "float4", "uint");

            compute = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Compute, GetSource(), "CubeSkippingShader");
        }

        public void Run(ITexture src, ITexture dst, LayerMipmapSlice lm, UploadBuffer upload)
        {
            var size = src.Size.GetMip(lm.Mipmap);
            var dev = Device.Get();

            initTexShader.Run(src, dst, lm, upload);

            // pong texture
            ITexture pong = new ImageFramework.DirectX.Texture3D(dst.NumMipmaps, dst.Size, Format.R8_UInt, true, false);


            dev.Compute.Set(compute.Compute);
            upload.SetData(size);
            dev.Compute.SetConstantBuffer(0, upload.Handle);

            ImageFramework.DirectX.Query.SyncQuery syncQuery = new ImageFramework.DirectX.Query.SyncQuery();

            for (int i = 0; i < 254; ++i)
            {
                // bind textures
                dev.Compute.SetShaderResource(0, dst.GetSrView(lm));
                dev.Compute.SetUnorderedAccessView(0, pong.GetUaView(lm.Mipmap));

                // execute
                dev.Dispatch(
                    Utility.DivideRoundUp(size.X, workgroupSize.X),
                    Utility.DivideRoundUp(size.Y, workgroupSize.Y),
                    Utility.DivideRoundUp(size.Z, workgroupSize.Z)
                );

                // unbind texture
                dev.Compute.SetShaderResource(0, null);
                dev.Compute.SetUnorderedAccessView(0, null);

                // swap textures
                var tmp = pong;
                pong = dst;
                dst = tmp;

#if DEBUG
                if (i % 8 == 0)
                {
                    syncQuery.Set();
                    syncQuery.WaitForGpu();
                    Console.WriteLine("Iteration: " + i);
                }
#endif
            }

            dev.Compute.Set(null);

            pong?.Dispose();
            syncQuery?.Dispose();
        }

        public void Dispose()
        {
            initTexShader?.Dispose();
        }

        private string GetSource()
        {
            return $@"
cbuffer DirBuffer : register(b0){{
    int3 size;
}};

Texture3D<uint> in_tex : register(t0);
RWTexture3D<uint> out_tex : register(u0);

[numthreads({workgroupSize.X},{workgroupSize.Y},{workgroupSize.Z})]
void main(int3 id : SV_DispatchThreadID) {{
    if(any(id >= size)) return; // outside
    if(in_tex[id] == 0) return; // stays zero

    uint minVal = 255;
    
    [unroll] for(uint i = 0; i < 3; ++i){{
        int3 offset = 0;
        offset[i] = 1;
        [flatten] if(id[i] > 0) minVal = min(minVal, in_tex[id - offset]);
        [flatten] if(id[i] + 1 < size[i]) minVal = min(minVal, in_tex[id + offset]);
    }}

    out_tex[id] = min(minVal + 1, 255);
}}
";
        }
    }
}
