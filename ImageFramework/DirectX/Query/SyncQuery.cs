using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace ImageFramework.DirectX.Query
{
    internal class SyncQuery : IDisposable
    {
        private readonly SharpDX.Direct3D11.Query query;
        private bool isActive = false;
        public SyncQuery()
        {
            var desc = new QueryDescription
            {
                Type = QueryType.Event,
                Flags = QueryFlags.None
            };

            query = new SharpDX.Direct3D11.Query(Device.Get().Handle, desc);
        }

        /// <summary>
        /// sets the query at this point in the pipeline
        /// </summary>
        public void Set()
        {
            if (isActive)
            {
                // wait for previous query to be finished
                var cts = new CancellationTokenSource();
                WaitForGpuAsync(cts.Token).Wait();
            }
            
            Device.Get().EndQuery(query);
            isActive = true;
        }

        /// <summary>
        /// waits for the query to finish
        /// </summary>
        public async Task WaitForGpuAsync(CancellationToken ct)
        {
            Debug.Assert(isActive);
            // flush before wait to ensure that commands were submitted
            Device.Get().Flush();

            int timeout = 1; // start with waiting 1 ms
            do
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(timeout);
                timeout = Math.Min(timeout * 2, 1000);
            } while (!Device.Get().GetQueryEventData(query));

            isActive = false;
        }

        public void Dispose()
        {
            query?.Dispose();
        }
    }
}
