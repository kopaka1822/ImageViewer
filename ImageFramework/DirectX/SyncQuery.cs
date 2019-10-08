using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace ImageFramework.DirectX
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
            /*if (isActive)
            {
                // wait for previous query to be finished
                var cts = new CancellationTokenSource();
                WaitForGpuAsync(cts.Token).Wait();
            }*/
            
            Device.Get().EndQuery(query);
            isActive = true;
        }

        /// <summary>
        /// waits for the query to finish
        /// </summary>
        public async Task WaitForGpuAsync(CancellationToken ct)
        {
            Debug.Assert(isActive);

            while (!Device.Get().GetQueryEventData(query))
            {
                await Task.Delay(10, ct);
            }
        }

        public void Dispose()
        {
            query?.Dispose();
        }
    }
}
