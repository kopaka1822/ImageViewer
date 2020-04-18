using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D11;
using System;
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


        public void Execute(ITexture orgTex, ITexture helpTex, LayerMipmapSlice lm, UploadBuffer uploadBuffer)
        {
            var size = orgTex.Size.GetMip(lm.Mipmap);
            var dev = Device.Get();


            initTexShader.Run(orgTex, helpTex, lm, uploadBuffer);

            Texture3D pong = new Texture3D(helpTex.NumMipmaps, helpTex.Size, Format.R8_UInt, true, false);
            // it seems that pong is not always cleared to 0 in the beginning => clear pong as well
            initTexShader.Run(orgTex, pong, lm, uploadBuffer);

            bool readHelpTex = false;

            dev.Compute.Set(compute.Compute);

            ImageFramework.DirectX.Query.SyncQuery syncQuery = new ImageFramework.DirectX.Query.SyncQuery();

            for (int i = 0; i < 127; i++)
            {

                swapTextures(ref readHelpTex, helpTex, pong, lm);

                //x-direction
                uploadBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(1, 0, 0),
                    dim = size
                });
                dev.Compute.SetConstantBuffer(0, uploadBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));


                swapTextures(ref readHelpTex, helpTex, pong, lm);

                //y-direction
                uploadBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(0, 1, 0),
                    dim = size
                });
                dev.Compute.SetConstantBuffer(0, uploadBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));

                swapTextures(ref readHelpTex, helpTex, pong, lm);

                //z-direction
                uploadBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(0, 0, 1),
                    dim = size
                });
                dev.Compute.SetConstantBuffer(0, uploadBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));

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


            endShader.Run(pong, helpTex, lm, uploadBuffer);
            pong?.Dispose();
            syncQuery?.Dispose();
        }

        private void swapTextures(ref bool readHelpTex, ITexture helpTex, ITexture pong, LayerMipmapSlice lm)
        {
            var dev = Device.Get();
            if (readHelpTex)
            {
                readHelpTex = false;

                dev.Compute.SetShaderResource(0, null);
                dev.Compute.SetUnorderedAccessView(0, null);

                dev.Compute.SetShaderResource(0, pong.GetSrView(lm));
                dev.Compute.SetUnorderedAccessView(0, helpTex.GetUaView(lm.Mipmap));

            }
            else
            {
                readHelpTex = true;

                dev.Compute.SetShaderResource(0, null);
                dev.Compute.SetUnorderedAccessView(0, null);

                dev.Compute.SetShaderResource(0, helpTex.GetSrView(lm));
                dev.Compute.SetUnorderedAccessView(0, pong.GetUaView(lm.Mipmap));
            }
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
