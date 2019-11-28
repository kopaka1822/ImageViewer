using System;
using System.Diagnostics;
using System.Threading;
using SharpDX.Direct3D11;

namespace ImageFramework.DirectX.Query
{
    public class GpuTimer : IDisposable
    {
        private SharpDX.Direct3D11.Query disjointHandle;
        private SharpDX.Direct3D11.Query startHandle;
        private SharpDX.Direct3D11.Query endHandle;
        private bool isRunning = false;

        public GpuTimer()
        {
            startHandle = new SharpDX.Direct3D11.Query(Device.Get().Handle, new QueryDescription
            {
                Flags = QueryFlags.None,
                Type = QueryType.Timestamp
            });

            endHandle = new SharpDX.Direct3D11.Query(Device.Get().Handle, new QueryDescription
            {
                Flags = QueryFlags.None,
                Type = QueryType.Timestamp
            });

            disjointHandle = new SharpDX.Direct3D11.Query(Device.Get().Handle, new QueryDescription
            {
                Flags = QueryFlags.None,
                Type = QueryType.TimestampDisjoint
            });
        }

        /// <summary>
        /// inserts timestamp on gpu
        /// </summary>
        public void Start()
        {
            Debug.Assert(!isRunning);
            Device.Get().Begin(disjointHandle);
            Device.Get().End(startHandle);
            isRunning = true;
        }

        /// <summary>
        /// inserts end timestamp on gpu
        /// </summary>
        public void Stop()
        {
            Debug.Assert(isRunning);
            Device.Get().End(endHandle);
            Device.Get().End(disjointHandle);
            isRunning = false;
        }

        /// <returns>true if data is available</returns>
        public bool HasData()
        {
            Debug.Assert(!isRunning);
            return Device.Get().ContextHandle.GetData<UInt64>(endHandle, out var tmp);
        }

        /// <summary>
        /// time delta in milliseconds
        /// </summary>
        public float GetDelta()
        {
            Debug.Assert(!isRunning);
            UInt64 t2;
            while (!Device.Get().ContextHandle.GetData<UInt64>(endHandle, out t2))
            {
                Thread.Sleep(1);
            }
            var freq = Device.Get().ContextHandle.GetData<QueryDataTimestampDisjoint>(disjointHandle);
            var t1 = Device.Get().ContextHandle.GetData<UInt64>(startHandle, AsynchronousFlags.None);

            Debug.Assert(!freq.Disjoint);

            return ((float) (t2 - t1) / (float) freq.Frequency) * 1000.0f; 
        }

        public void Dispose()
        {
            startHandle?.Dispose();
            endHandle?.Dispose();
            disjointHandle?.Dispose();
        }
    }
}
