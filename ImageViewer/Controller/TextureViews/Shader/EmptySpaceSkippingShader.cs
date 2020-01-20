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
        private ImageFramework.DirectX.Shader compute;
        private static readonly Size3 workgroupSize = new Size3(8, 8, 8);

        public EmptySpaceSkippingShader()
        {
            this.compute = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Compute, GetComputeSource(), "Empty-Space-Skipping Shader");
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

            SpaceSkippingTexture3D pong = new RayCastingView.SpaceSkippingTexture3D(helpTex.texSize,helpTex.numMipMaps);


            bool currentTexIshelpTex = true;
            dev.Compute.SetShaderResource(1, helpTex.GetSrView(0));
            dev.Compute.SetUnorderedAccessView(0, pong.GetUaView(0));


            for (int i = 0; i < 255; i++)
            {
                //x-direction
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(1, 0, 0)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(size.X / workgroupSize.X, size.Y / workgroupSize.Y, size.Z / workgroupSize.Z);

                //y-direction
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(0, 1, 0)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(size.X / workgroupSize.X, size.Y / workgroupSize.Y, size.Z / workgroupSize.Z);

                //z-direction
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(0, 0, 1)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(size.X / workgroupSize.X, size.Y / workgroupSize.Y, size.Z / workgroupSize.Z);

                if (currentTexIshelpTex)
                {
                    dev.Compute.SetShaderResource(1, helpTex.GetSrView(0));
                    dev.Compute.SetUnorderedAccessView(0, pong.GetUaView(0));
                    currentTexIshelpTex = false;
                }
                else
                {
                    dev.Compute.SetShaderResource(1, pong.GetSrView(0));
                    dev.Compute.SetUnorderedAccessView(0, helpTex.GetUaView(0));
                    currentTexIshelpTex = true;
                }
            }
          
            DebugTex(pong);



        }



        private static string GetComputeSource()
        {
            return $@"

cbuffer DirBuffer : register(b0){{
    int3 dir;
}};

Texture3D<float4> OrgInput: register(t0);
Texture3D<uint> HelpTexOutput: register(t1);
RWTexture3D<uint> Pong: register(u0);


{NumThreads()}
void main(uint3 dispatchThreadID : SV_DispatchThreadID)
{{
    uint width, height, depth;
    OrgInput.GetDimensions(width, height, depth);
    uint3 dim = uint3(width,height, depth);

    float4 src = OrgInput[dispatchThreadID];
    if(src.a > 0){{
        Pong[dispatchThreadID] = 0;    
    }}    
    else{{
        uint sample1, sample2;
        uint3 sample1Point = dispatchThreadID + dir;
        uint3 sample2Point = dispatchThreadID - dir;

        if( all(sample1Point<= dim) && all(sample1Point  >= uint3(0,0,0)) ){{
            sample1 = HelpTexOutput[dispatchThreadID + dir];    
        }}
        else{{
            sample1 = 255;
        }}
        if( all(sample2Point<= dim) && all(sample2Point  >= uint3(0,0,0)) ){{
            sample2 = HelpTexOutput[dispatchThreadID - dir];
        }}
        else{{
            sample2 = 255;
        }}

        Pong[dispatchThreadID] = min(HelpTexOutput[dispatchThreadID], min(sample1,sample2)) + 1;
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
                Width = tex.texSize.Width,
                Height = tex.texSize.Height,
                Depth = tex.texSize.Depth,
                Format = SharpDX.DXGI.Format.R8_UInt,
                MipLevels = 1,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Staging
            };

            // create staging texture
            var staging = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, desc);

            // copy data to staging texture
            Device.Get().CopySubresource(tex.texHandle, staging, 0, 0, tex.texSize);
            
            var tmp = Device.Get().GetData(staging, 0, tex.texSize, 1);


        }

    }
}
