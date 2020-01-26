
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;

namespace ImageFramework.Model.Shader
{
    public class ReduceShader : IDisposable
    {
        private static readonly int LocalSize = 64;
        private static readonly int ElementsPerThread = 8;
        public static readonly int ElementsPerGroup = ElementsPerThread * LocalSize;
        private readonly string type;
        private readonly string reduceOp;
        private readonly string defaultValue;

        private readonly DirectX.Shader shader;
        private readonly UploadBuffer numBuffer;

        public ReduceShader(UploadBuffer upload, string reduceOp = "a+b", string defaultValue = "0.0", string type = "float")
        {
            this.type = type;
            this.reduceOp = reduceOp;
            this.defaultValue = defaultValue;

            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(), "ReduceShader");
            Debug.Assert(upload.ByteSize >= 4);
            numBuffer = upload;
        }

        /// <summary>
        /// performs a reduce on the buffer. The final value will be at buffer[0]
        /// </summary>
        /// <param name="numElements">number of elements for the reduce</param>
        public void Run(GpuBuffer buffer, int numElements)
        {
            Device.Get().Compute.Set(shader.Compute);
            Device.Get().Compute.SetUnorderedAccessView(0, buffer.View);

            while (numElements > 1)
            {
                int numGroups = Utility.Utility.DivideRoundUp(numElements, ElementsPerGroup);
                numBuffer.SetData(numElements);
                Device.Get().Compute.SetConstantBuffer(0, numBuffer.Handle);

                // test if numGroups > DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION
                var numSplits = Utility.Utility.DivideRoundUp(numGroups, Device.DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION);
                if(numSplits > 1) Device.Get().Dispatch(Device.DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION, numSplits);
                else Device.Get().Dispatch(numGroups, 1);

                numElements = numGroups;
            }

            Device.Get().Compute.Set(null);
        }

        private string GetSource()
        {
            return $@"
RWStructuredBuffer<{type}> buffer : register(u0);

groupshared {type} cache[{LocalSize}];

{type} reduce({type} a, {type} b)
{{
    return {reduceOp};
}}

cbuffer BufferData : register(b0)
{{
    uint u_bufferSize;
}}

[numthreads({LocalSize}, 1, 1)]
void main(uint3 localInvocationID : SV_GroupThreadID, uint3 workGroupID : SV_GroupID)
{{
    uint localIndex = localInvocationID.x;
    // reconstruct 1D workgroupID
    uint workGroupId = workGroupID.x + workGroupID.y * {Device.DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION};
    uint globalIndex = workGroupId * {LocalSize} + localIndex;

    {type} res = {defaultValue};
    uint offset = workGroupId * {ElementsPerGroup} + localIndex;

    // read in local data
    [unroll] for(uint i = 0; i < {ElementsPerThread}; ++i)
    {{
        {type} tmp;
        [flatten] if(offset < u_bufferSize)
            tmp = buffer[offset];
        else
            tmp = {defaultValue};
        res = reduce(res, tmp);
        offset += {LocalSize};
    }}

    // write to shared memory
    cache[localIndex] = res;

    // reduce to cache[0]
    uint stride = {LocalSize} / 2;
    [unroll] while(stride >= 1)
    {{
        GroupMemoryBarrierWithGroupSync();
        
        if(localIndex < stride)
            cache[localIndex] = reduce(cache[localIndex], cache[localIndex + stride]);
        stride /= 2;
    }}

    // write back result
    if(localIndex != 0) return;
    buffer[workGroupId] = cache[0];
}}
";
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
