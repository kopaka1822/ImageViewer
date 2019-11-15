
using System;
using System.CodeDom;
using System.Collections.Generic;
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
        private readonly UploadBuffer<int> numBuffer;

        public ReduceShader(string reduceOp = "a+b", string defaultValue = "0.0", string type = "float")
        {
            this.type = type;
            this.reduceOp = reduceOp;
            this.defaultValue = defaultValue;

            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(), "ReduceShader");
            numBuffer = new UploadBuffer<int>(1);
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

                Device.Get().Dispatch(numGroups, 1);
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
    uint globalIndex = workGroupID.x * {LocalSize} + localIndex;

    {type} local[{ElementsPerThread}];
    uint offset = workGroupID.x * {ElementsPerGroup} + localIndex;
    // read in local data
    [unroll] for(uint i = 0; i < {ElementsPerThread}; ++i)
    {{
        [flatten] if(offset < u_bufferSize)
            local[i] = buffer[offset];
        else
            local[i] = {defaultValue};
        offset += {LocalSize};
    }}
    
    {type} res = local[0];
    // perform local reduce
    [unroll] for(i = 1; i < {ElementsPerThread}; ++i)
    {{
        res = reduce(res, local[i]);
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
    buffer[workGroupID.x] = cache[0];
}}
";
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
