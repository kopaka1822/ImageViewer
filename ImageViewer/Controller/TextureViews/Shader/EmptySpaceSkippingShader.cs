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
        private ImageFramework.DirectX.Shader computeMin;
        private ImageFramework.DirectX.Shader computePlusOne;
        private static readonly Size3 workgroupSize = new Size3(8, 8, 8);

        public EmptySpaceSkippingShader()
        {
            this.computeMin = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Compute, GetMinComputeSource(), "Min Filter");
            this.computePlusOne = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Compute, GetPlusOneComputeSource(), "Plus one");
        }
        private struct DirBufferData
        {
            public Int3 dir;
        }

        public void Dispose()
        {
            computeMin?.Dispose();
            computePlusOne?.Dispose();
        }


        public void Execute(ShaderResourceView orgTex, SpaceSkippingTexture3D helpTex, Size3 size)
        {

            var dev = Device.Get();
            dev.Compute.SetShaderResource(0, orgTex);

            UploadBuffer directionBuffer = new UploadBuffer(Int3.SizeInBytes);

            SpaceSkippingTexture3D pong = new RayCastingView.SpaceSkippingTexture3D(helpTex.texSize, helpTex.numMipMaps);


            bool readHelpTex = true;
            dev.Compute.SetShaderResource(1, helpTex.GetSrView(0));
            dev.Compute.SetUnorderedAccessView(0, pong.GetUaView(0));


            for (int i = 0; i < 255; i++)
            {
                dev.Compute.Set(computeMin.Compute);
                //x-direction
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(1, 0, 0)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));

                swapTextures(readHelpTex, helpTex, pong);


                //y-direction
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(0, 1, 0)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));

                swapTextures(readHelpTex, helpTex, pong);

                //z-direction 
                //and +1 on at the end
                dev.Compute.Set(computePlusOne.Compute);
                directionBuffer.SetData(new DirBufferData
                {
                    dir = new Int3(0, 0, 1)
                });
                dev.Compute.SetConstantBuffer(0, directionBuffer.Handle);
                dev.Dispatch(Utility.DivideRoundUp(size.X, workgroupSize.X), Utility.DivideRoundUp(size.Y, workgroupSize.Y), Utility.DivideRoundUp(size.Z, workgroupSize.Z));


                swapTextures(readHelpTex, helpTex, pong);

                if (readHelpTex)
                {
                    //DebugTex(helpTex);
                }
                else
                {
                    //DebugTex(pong);
                }
            }

            DebugTex(helpTex);



        }

        private void swapTextures(bool readHelpTex, SpaceSkippingTexture3D helpTex, SpaceSkippingTexture3D pong)
        {
            var dev = Device.Get();
            if (readHelpTex)
            {
                readHelpTex = false;
                dev.Compute.SetShaderResource(1, pong.GetSrView(0));
                dev.Compute.SetUnorderedAccessView(0, helpTex.GetUaView(0));
            }
            else
            {
                readHelpTex = true;
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
    uint3 dim = uint3(width,height, depth);

    float4 src = OrgInput[dispatchThreadID];
    if(src.a > 0){{
        CurrentWrite[dispatchThreadID] = 0;    
    }}    
    else{{
        uint sample1, sample2;
        uint3 sample1Point = dispatchThreadID + dir;
        uint3 sample2Point = dispatchThreadID - dir;

        if( all(sample1Point < dim) && all(sample1Point  > 0) ){{
            sample1 = CurrentRead[dispatchThreadID + dir];    
        }}
        else{{
            sample1 = 255;
        }}
        if( all(sample2Point < dim) && all(sample2Point > 0 ) ){{
            sample2 = CurrentRead[dispatchThreadID - dir];
        }}
        else{{
            sample2 = 255;
        }}
        CurrentWrite[dispatchThreadID] = min(CurrentRead[dispatchThreadID], min(sample1,sample2));
    }}
}}

";
        }

        private static string GetPlusOneComputeSource()
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
    uint3 dim = uint3(width,height, depth);

    float4 src = OrgInput[dispatchThreadID];
    if(src.a > 0){{
        CurrentWrite[dispatchThreadID] = 0;    
    }}    
    else{{
        uint sample1, sample2;
        uint3 sample1Point = dispatchThreadID + dir;
        uint3 sample2Point = dispatchThreadID - dir;

        if( all(sample1Point < dim) && all(sample1Point  > 0) ){{
            sample1 = CurrentRead[dispatchThreadID + dir];    
        }}
        else{{
            sample1 = 255;
        }}
        if( all(sample2Point < dim) && all(sample2Point > 0 ) ){{
            sample2 = CurrentRead[dispatchThreadID - dir];
        }}
        else{{
            sample2 = 255;
        }}
        CurrentWrite[dispatchThreadID] = min(CurrentRead[dispatchThreadID], min(sample1,sample2)) + 1;
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

            var tmp = tex.GetPixelColors(0, 0);


        }

    }
}
