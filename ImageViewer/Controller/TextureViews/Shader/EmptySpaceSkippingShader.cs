using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace ImageViewer.Controller.TextureViews.Shader
{
    using Device = ImageFramework.DirectX.Device;

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


        public void Execute(ShaderResourceView orgTex, UnorderedAccessView helpTex, Size3 size)
        {
            var dev = Device.Get();
            dev.Compute.Set(compute.Compute);
            dev.Compute.SetShaderResource(0, orgTex);
            dev.Compute.SetUnorderedAccessView(0, helpTex);
            UploadBuffer directionBuffer = new UploadBuffer(Int3.SizeInBytes);

            //x-direction
            directionBuffer.SetData(new DirBufferData
            {
                dir = new Int3(1, 0, 0)
            });
            dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
            for (int i = 0; i < 255; i++)
            {
                dev.Dispatch(size.X / workgroupSize.X, size.Y / workgroupSize.Y, size.Z / workgroupSize.Z);
            }


            //y-direction
            directionBuffer.SetData(new DirBufferData
            {
                dir = new Int3(0, 1, 0)
            });
            dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);

            for (int i = 0; i < 255; i++)
            {
                dev.Dispatch(size.X / workgroupSize.X, size.Y / workgroupSize.Y, size.Z / workgroupSize.Z);
            }



            //z-direction
            directionBuffer.SetData(new DirBufferData
            {
                dir = new Int3(0, 0, 1)
            });
            dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);

            for (int i = 0; i < 255; i++)
            {
                dev.Dispatch(size.X / workgroupSize.X, size.Y / workgroupSize.Y, size.Z / workgroupSize.Z);
            }



        }



        private static string GetComputeSource()
        {
            return $@"

cbuffer DirBuffer : register(b0){{
    int3 dir;
}};

Texture3D<float4> OrgInput: register(t0);
RWTexture3D<uint> HelpTexOutput: register(u0);


{NumThreads()}
void main(uint3 dispatchThreadID : SV_DispatchThreadID)
{{
    float4 src = OrgInput[dispatchThreadID];
    if(src.a > 0.1){{
        HelpTexOutput[dispatchThreadID] = 0;    
    }}    
    else{{
        uint sample1 = HelpTexOutput[dispatchThreadID + dir];    
        uint sample2 = HelpTexOutput[dispatchThreadID - dir];
        HelpTexOutput[dispatchThreadID] = min(HelpTexOutput[dispatchThreadID], min(sample1,sample2)) + 1;
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



    }
}
