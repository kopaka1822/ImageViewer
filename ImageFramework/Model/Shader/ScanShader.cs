using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.DirectX;

namespace ImageFramework.Model.Shader
{
    public class ScanShader : IDisposable
    {
        private struct ShaderData
        {
            public int BufferSize;
            public int WriteAux;
        }

        private static readonly int localSize = 1024;
        private static readonly int elementsPerScan = 8; // 2 vec4's are processed in one invocation
        private static readonly int BufferAlignment = localSize * elementsPerScan;
        private static readonly int localSizePush = 64;
       

        private readonly DirectX.Shader scanShader;
        private readonly DirectX.Shader pushShader;

        private readonly List<GpuBuffer> auxBuffers = new List<GpuBuffer>();
        private int lastNumElements = 0;
        private readonly UploadBuffer<ShaderData> cBuffer = new UploadBuffer<ShaderData>();

        // \param type used for the scan. Must be scalar type because 
        public ScanShader(string type = "float")
        {
            scanShader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetPushSource(type), "ScanShader");
            pushShader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetPushSource(type), "PushShader");
        }

        public void Run(GpuBuffer buffer, int numElements)
        {
            Debug.Assert(buffer.ElementSize == 4);

            var scanLastElementIdx = numElements - 1;
            // check if there are enough aux buffers
            Setup(numElements, buffer.ElementCount);

            PerformScan(buffer, numElements);
            PerformPush(buffer);
            // unbind resources
            var dev = Device.Get();
            dev.Compute.Set(null);
            dev.Compute.SetUnorderedAccessView(0, null);
            dev.Compute.SetUnorderedAccessView(1, null);
            dev.Compute.SetUnorderedAccessView(2, null);
        }

        /// <summary>
        /// element alignment for the specified element count
        /// </summary>
        public int GetSourceBufferAlignment(int numElements)
        {
            var curScanSize = Utility.Utility.AlignTo(numElements, BufferAlignment);
            return curScanSize;
        }

        private void PerformScan(GpuBuffer buffer, int numElements)
        {
            var bs = buffer.ElementCount;
            int i = 0;

            var dev = Device.Get();
            dev.Compute.Set(scanShader.Compute);

            while (bs > 0)
            {
                // set source
                if(i == 0) dev.Compute.SetUnorderedAccessView(0, buffer.View);
                else dev.Compute.SetUnorderedAccessView(0, auxBuffers[i].View);

                // set destination
                dev.Compute.SetUnorderedAccessView(1, auxBuffers[i].View);

                // Bind the auxiliary buffer for the next step 
                if (i + i < auxBuffers.Count)
                    dev.Compute.SetUnorderedAccessView(2, auxBuffers[i + 1].View);

                // update constants
                cBuffer.SetData(new ShaderData
                {
                    BufferSize = auxBuffers[i].ElementCount,
                    WriteAux = i == auxBuffers.Count ? 1 : 0
                });
                dev.Compute.SetConstantBuffer(0, cBuffer.Handle);

                // perform scan 
                dev.Dispatch(Utility.Utility.DivideRoundUp(bs, BufferAlignment), 1);

                bs /= BufferAlignment;
                ++i;
            }
        }

        private void PerformPush(GpuBuffer buffer)
        {
            var dev = Device.Get();
            dev.Compute.Set(pushShader.Compute);
            cBuffer.SetData(new ShaderData
            {
                BufferSize = BufferAlignment // in this case its the stride
            });
            dev.Compute.SetConstantBuffer(0, cBuffer.Handle);

            var i = auxBuffers.Count - 1;
            var bs = buffer.ElementCount;
            while (bs > BufferAlignment) bs /= BufferAlignment;
            while (bs < buffer.ElementCount)
            {
                bs *= BufferAlignment;
                
                dev.Compute.SetUnorderedAccessView(0, auxBuffers[i].View);
                dev.Compute.SetUnorderedAccessView(1, auxBuffers[i - 1].View);

                dev.Dispatch((bs - BufferAlignment) / localSizePush, 1);

                --i;
            }
        }

        private void Setup(int numElements, int numAligned)
        {
            Debug.Assert(numAligned == GetSourceBufferAlignment(numElements));

            if (numElements == lastNumElements) return; // buffers already exist

            // create aux buffer
            ClearAux();
            var bs = numAligned;
            while (bs > 1)
            {
                auxBuffers.Add(new GpuBuffer(4, Utility.Utility.AlignTo(bs, 4)));
                bs /= BufferAlignment;
            }
        }

        private void ClearAux()
        {
            foreach (var auxBuffer in auxBuffers)
            {
                auxBuffer?.Dispose();
            }
            auxBuffers.Clear();
        }

        private static string GetScanSource(string type)
        {
            return $@"
RWStructuredBuffer<{type}> in_buffer : register(u0);
RWStructuredBuffer<{type}> data : register(u1);

RWStructuredBuffer<{type}> aux : register(u2);

cbuffer BufferData : register(b0)
{{
	uint u_bufferSize;
	uint u_writeAux;
}};

groupshared {type} s_temp[{localSize} * 2];
groupshared {type} s_blockSums[{localSize} / 32];

// Inclusive scan which produces the prefix sums for 64 elements in parallel
void intraWarpScan(uint threadID)
{{
	int id = threadID * 2;
	s_temp[id + 1] += s_temp[id];
	
	id = (threadID | 1) << 1;
	s_temp[id + (threadID & 1)] += s_temp[id - 1];

	id = ((threadID >> 1) | 1) << 2;
	s_temp[id + (threadID & 3)] += s_temp[id - 1];

	id = ((threadID >> 2) | 1) << 3;
	s_temp[id + (threadID & 7)] += s_temp[id - 1];

	id = ((threadID >> 3) | 1) << 4;
	s_temp[id + (threadID & 15)] += s_temp[id - 1];

	id = ((threadID >> 4) | 1) << 5;
	s_temp[id + (threadID & 31)] += s_temp[id - 1];
}}

void intraBlockScan(uint threadID)
{{
	int id = threadID * 2;
	s_blockSums[id + 1] += s_blockSums[id];
	
	id = (threadID | 1) << 1;
	s_blockSums[id + (threadID & 1)] += s_blockSums[id - 1];

	id = ((threadID >> 1) | 1) << 2;
	s_blockSums[id + (threadID & 3)] += s_blockSums[id - 1];

	id = ((threadID >> 2) | 1) << 3;
	s_blockSums[id + (threadID & 7)] += s_blockSums[id - 1];

	id = ((threadID >> 3) | 1) << 4;
	s_blockSums[id + (threadID & 15)] += s_blockSums[id - 1];
}}

void writeData(uint idx, {type}4 value)
{{
    data[idx] = value.x;
    data[idx+1] = value.y;
    data[idx+2] = value.z;
    data[idx+3] = value.w;
}}

{type}4 readData(uint idx) 
{{
    {type}4 res;
    res.x = in_buffer[idx];
    res.y = in_buffer[idx+1];
    res.z = in_buffer[idx+2];
    res.w = in_buffer[idx+3];
    return res;
}}

[numthreads({localSize}, 1, 1)]
void main(uint3 localInvocationID : SV_GroupThreadID, uint3 workGroupID : SV_GroupID)
{{
	uint threadID = localInvocationID.x;
	uint idx = (workGroupID.x * {localSize} * 2 + threadID) * 4;
	{type}4 inputValuesA = readData(idx);
	{type}4 inputValuesB = readData(idx + {localSize}*4);
	inputValuesA.y += inputValuesA.x;
	inputValuesA.z += inputValuesA.y;
	inputValuesA.w += inputValuesA.z;
	s_temp[threadID] = inputValuesA.w;
	inputValuesB.y += inputValuesB.x;
	inputValuesB.z += inputValuesB.y;
	inputValuesB.w += inputValuesB.z;
	s_temp[threadID + {localSize}] = inputValuesB.w;
	GroupMemoryBarrierWithGroupSync();
	
	// 1. Intra-warp scan in each warp
	intraWarpScan(threadID);
	GroupMemoryBarrierWithGroupSync();
	
	// 2. Collect per-warp sums
	if(threadID < ({localSize}/32))
		s_blockSums[threadID] = s_temp[threadID * 64 + 63];
		
	// 3. Use 1st warp to scan per-warp results
	if(threadID < ({localSize}/64))
		intraBlockScan(threadID);
	GroupMemoryBarrierWithGroupSync();
	
	// 4. Add new warp offsets from step 3 to the results
	{type} blockOffset = threadID < 64 ? 0 : s_blockSums[threadID / 64 - 1];
	{type} val = s_temp[threadID] + blockOffset;
	if(idx < u_bufferSize)
        writeData(idx, {type}4(val - inputValuesA.w + inputValuesA.xyz, val));
	
	blockOffset = s_blockSums[(threadID + {localSize}) / 64 - 1];
	val = s_temp[threadID + {localSize}] + blockOffset;

	if(idx + {localSize}*4 < u_bufferSize)
        writeData(idx+{localSize}*4, {type}4(val - inputValuesB.w + inputValuesB.xyz, val));
	
	// 5. The last thread in each block must return into the (thickly packed) auxiliary array
	if(threadID == {localSize}-1 && u_writeAux)
		aux[workGroupID.x] = val;
}}
";
        }

        private static string GetPushSource(string type)
        {
            return $@"
RWStructuredBuffer<{type}> aux_data : register(u0);
RWStructuredBuffer<{type}> data : register(u1);

cbuffer BufferData : register(b0)
{{
    uint u_stride;
}};

[numthreads({localSizePush}, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{{
    data[globalInvocationID.x + u_stride] += aux_data[int(globalInvocationID.x / u_stride)];
}}
";
        }

        public void Dispose()
        {
            ClearAux();
            scanShader?.Dispose();
            pushShader?.Dispose();

        }
    }
}
