using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace ImageFramework.DirectX
{
    public class GpuTimer : IDisposable
    {
        private Query disjointHandle;
        private Query startHandle;
        private Query endHandle;
        private bool isRunning = false;

        public GpuTimer()
        {
            startHandle = new Query(Device.Get().Handle, new QueryDescription
            {
                Flags = QueryFlags.None,
                Type = QueryType.Timestamp
            });

            endHandle = new Query(Device.Get().Handle, new QueryDescription
            {
                Flags = QueryFlags.None,
                Type = QueryType.Timestamp
            });

            disjointHandle = new Query(Device.Get().Handle, new QueryDescription
            {
                Flags = QueryFlags.None,
                Type = QueryType.TimestampDisjoint
            });
        }

        public void Start()
        {
            Debug.Assert(!isRunning);
            Device.Get().Begin(disjointHandle);
            Device.Get().End(startHandle);
            isRunning = true;
        }

        public void Stop()
        {
            Debug.Assert(isRunning);
            Device.Get().End(endHandle);
            Device.Get().End(disjointHandle);
            isRunning = false;
        }

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
