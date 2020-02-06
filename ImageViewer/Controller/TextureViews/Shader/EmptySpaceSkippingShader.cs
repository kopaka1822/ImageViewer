using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using ImageViewer.Controller.TextureViews.Texture3D;

namespace ImageViewer.Controller.TextureViews.Shader
{
    using Device = ImageFramework.DirectX.Device;
    using SpaceSkippingTexture3D = RayCastingView.SpaceSkippingTexture3D;

    public class EmptySpaceSkippingShader : IDisposable
    {
        private readonly ImageFramework.DirectX.Shader compute;
        private static readonly Size3 workgroupSize = new Size3(8, 8, 8);

        public EmptySpaceSkippingShader()
        {
            this.compute = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Compute, GetMinComputeSource(), "Empty-Space-Skipping Shader");
        }
        private struct DirBufferData
        {
            public Int3 dir;
        }

        public void Dispose()
        {
            compute?.Dispose();
        }


        public void Execute(ShaderResourceView orgTex, SpaceSkippingTexture3D helpTex, Size3 size)
        {

            var dev = Device.Get();
            dev.Compute.Set(compute.Compute);
            dev.Compute.SetShaderResource(0, orgTex);

            UploadBuffer directionBuffer = new UploadBuffer(Int3.SizeInBytes);

            SpaceSkippingTexture3D pong = new SpaceSkippingTexture3D(helpTex.Size, helpTex.NumMipmaps);

            bool readHelpTex = true;
            dev.Compute.SetShaderResource(1, helpTex.GetSrView(0));
            dev.Compute.SetUnorderedAccessView(0, pong.GetUaView(0));


            for (int i = 0; i < 255; i++)
            {
                swapTextures(ref readHelpTex, helpTex, pong);

                //x-direction
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(1, 0, 0)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));


                swapTextures(ref readHelpTex, helpTex, pong);

                //y-direction
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(0, 1, 0)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));

                swapTextures(ref readHelpTex, helpTex, pong);

                //z-direction
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(0, 0, 1)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));



            }

            //DebugTex(helpTex);
           

            // unbind
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetShaderResource(1, null);
            dev.Compute.SetUnorderedAccessView(0, null);
            dev.Compute.Set(null);
            pong?.Dispose();
        }

        private void swapTextures(ref bool readHelpTex, SpaceSkippingTexture3D helpTex, SpaceSkippingTexture3D pong)
        {
            var dev = Device.Get();
            if (readHelpTex)
            {
                readHelpTex = false;

                dev.Compute.SetShaderResource(1, null);
                dev.Compute.SetUnorderedAccessView(0, null);

                dev.Compute.SetShaderResource(1, pong.GetSrView(0));
                dev.Compute.SetUnorderedAccessView(0, helpTex.GetUaView(0));

            }
            else
            {
                readHelpTex = true;

                dev.Compute.SetShaderResource(1, null);
                dev.Compute.SetUnorderedAccessView(0, null);

                dev.Compute.SetShaderResource(1, helpTex.GetSrView(0));
                dev.Compute.SetUnorderedAccessView(0, pong.GetUaView(0));
            }
        }

        private static string GetMinComputeSource()
        {
            return $@"

cbuffer DirBuffer : register(b0){{
    int3 dir;
}};

Texture3D<float4> OrgInput: register(t0);
Texture3D<uint> CurrentRead: register(t1);
RWTexture3D<uint> CurrentWrite: register(u0);


{NumThreads()}
void main(uint3 dispatchThreadID : SV_DispatchThreadID)
{{
    uint width, height, depth;
    OrgInput.GetDimensions(width, height, depth);
    uint3 dim = uint3(width,height,depth);

    float4 src = OrgInput[dispatchThreadID];
    if(src.a > 0){{
        CurrentWrite[dispatchThreadID] = 0;    
    }}    
    else{{
        uint3 sample1Point = dispatchThreadID + dir;
        uint3 sample2Point = dispatchThreadID - dir;

        uint sample1 = 255, sample2 = 255;

        if( all(sample1Point < dim) && all(sample1Point >= 0) ){{
            sample1 = CurrentRead[sample1Point];
        }}
        if( all(sample2Point < dim) && all(sample2Point >= 0 ) ){{
            sample2 = CurrentRead[sample2Point];
        }}
        if(dir.z != 1){{
            CurrentWrite[dispatchThreadID] = min(CurrentRead[dispatchThreadID], min(sample1,sample2));
        }}
        else{{
            CurrentWrite[dispatchThreadID] = min(CurrentRead[dispatchThreadID], min(sample1,sample2)) + 1;
        }}
    }}
}}

";
        }


        private static string NumThreads()
        {
            return $@"
        [numthreads({workgroupSize.X},{workgroupSize.Y},{workgroupSize.Z})]
";
        }

        private void DebugTex(SpaceSkippingTexture3D tex)
        {
            var desc = new Texture3DDescription
            {
                Width = tex.Size.Width,
                Height = tex.Size.Height,
                Depth = tex.Size.Depth,
                Format = SharpDX.DXGI.Format.R8_UInt,
                MipLevels = 1,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Staging
            };

            var tmp = tex.GetPixelColors(0, 0);
            int a = 0;
        }
    }
}
