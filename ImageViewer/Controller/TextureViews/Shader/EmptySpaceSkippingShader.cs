using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics;
using ImageFramework.Model.Shader;
using ImageViewer.Controller.TextureViews.Texture3D;
using SharpDX.DXGI;

namespace ImageViewer.Controller.TextureViews.Shader
{
    using Device = ImageFramework.DirectX.Device;
    using Texture3D = ImageFramework.DirectX.Texture3D;

    public class EmptySpaceSkippingShader : IDisposable
    {
        private readonly ImageFramework.DirectX.Shader compute;
        private static readonly Size3 workgroupSize = new Size3(8, 8, 8);
        private TransformShader initTexShader = new ImageFramework.Model.Shader.TransformShader("return value.a > 0 ? 0 : 255", "float4", "uint");
        private TransformShader endShader = new ImageFramework.Model.Shader.TransformShader("return value & 127", "uint", "uint");


        public EmptySpaceSkippingShader()
        {
            this.compute = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Compute, GetMinComputeSource(), "Empty-Space-Skipping Shader");
        }
        private struct DirBufferData
        {
            public Int3 dir;
#pragma warning disable 169
            private int padding;
#pragma warning restore 169
            public Size3 dim;
        }

        public void Dispose()
        {
            compute?.Dispose();
            initTexShader?.Dispose();
        }


        public void Run(ITexture orgTex, ITexture dstTex, ITexture tmpTex, LayerMipmapSlice lm, UploadBuffer uploadBuffer)
        {
            Debug.Assert(dstTex.Format == Format.R8_UInt);
            Debug.Assert(tmpTex.Format == Format.R8_UInt);

            var size = orgTex.Size.GetMip(lm.Mipmap);
            var dev = Device.Get();

            // remember for debugging
            var originalDst = dstTex;

            initTexShader.Run(orgTex, dstTex, lm, uploadBuffer);
            initTexShader.Run(orgTex, tmpTex, lm, uploadBuffer);

            dev.Compute.Set(compute.Compute);

            ImageFramework.DirectX.Query.SyncQuery syncQuery = new ImageFramework.DirectX.Query.SyncQuery();

            for (int i = 0; i < 127; i++)
            {
                BindAndSwapTextures(ref dstTex, ref tmpTex, lm);
                Dispatch(new Int3(1, 0, 0), size, uploadBuffer);

                BindAndSwapTextures(ref dstTex, ref tmpTex, lm);
                Dispatch(new Int3(0, 1, 0), size, uploadBuffer);

                BindAndSwapTextures(ref dstTex, ref tmpTex, lm);
                Dispatch(new Int3(0, 0, 1), size, uploadBuffer);

#if DEBUG
                if (i % 8 == 0)
                {
                    syncQuery.Set();
                    syncQuery.WaitForGpu();
                    Console.WriteLine("Iteration: " + i);
                }
#endif
            }

            // unbind
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);
            dev.Compute.Set(null);

            Debug.Assert(ReferenceEquals(originalDst, tmpTex));
            endShader.Run(dstTex, tmpTex, lm, uploadBuffer);
            syncQuery?.Dispose();
        }

        /// <summary>
        /// binds dstTex as source and tmpTex as destination and then swaps the references.
        /// => after rendering, dst tex will be the texture with the result
        /// </summary>
        /// <param name="dstTex"></param>
        /// <param name="tmpTex"></param>
        /// <param name="lm"></param>
        private void BindAndSwapTextures(ref ITexture dstTex, ref ITexture tmpTex, LayerMipmapSlice lm)
        {
            var dev = Device.Get();
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);

            dev.Compute.SetShaderResource(0, dstTex.GetSrView(lm));
            dev.Compute.SetUnorderedAccessView(0, tmpTex.GetUaView(lm.Mipmap));

            var tmp = tmpTex;
            tmpTex = dstTex;
            dstTex = tmp;
        }

        private void Dispatch(Int3 direction, Size3 size, UploadBuffer uploadBuffer)
        {
            var dev = Device.Get();
            uploadBuffer.SetData(new DirBufferData
            {
                dir = direction,
                dim = size
            });
            dev.Compute.SetConstantBuffer(0, uploadBuffer.Handle);
            dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));
        }

        private static string GetMinComputeSource()
        {
            return $@"

cbuffer DirBuffer : register(b0){{
    int3 dir;
    int padding;
    uint3 dim;
}};

Texture3D<uint> CurrentRead: register(t0);
RWTexture3D<uint> CurrentWrite: register(u0);


{NumThreads()}
void main(uint3 dispatchThreadID : SV_DispatchThreadID)
{{

    if(CurrentRead[dispatchThreadID] == 0)
        return;
 
    uint3 sample1Point = dispatchThreadID + dir;
    uint3 sample2Point = dispatchThreadID - dir;

    uint sample1 = 127, sample2 = 127;

    if( all(sample1Point < dim) && all(sample1Point >= 0) ){{
        sample1 = CurrentRead[sample1Point] & 127;
    }}
    if( all(sample2Point < dim) && all(sample2Point >= 0 ) ){{
        sample2 = CurrentRead[sample2Point] & 127;
    }}
    CurrentWrite[dispatchThreadID] = min ( min(CurrentRead[dispatchThreadID] & 127, min(sample1,sample2) ) + dir.z , 127 ) | 128 ;
}}

";
        }





        private static string NumThreads()
        {
            return $@"
        [numthreads({workgroupSize.X},{workgroupSize.Y},{workgroupSize.Z})]
";
        }
    }
}
