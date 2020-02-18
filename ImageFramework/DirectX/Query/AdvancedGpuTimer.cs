using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.DirectX.Query
{
    public class AdvancedGpuTimer : IDisposable
    {
        private readonly Queue<GpuTimer> activeTimers = new Queue<GpuTimer>();
        private readonly Stack<GpuTimer> cache = new Stack<GpuTimer>();
        private GpuTimer current = null;
        private float sum = 0.0f;
        private int count = 0;
        private float last = 0.0f;
        //private readonly List<float> times = new List<float>();

        public struct Stats
        {
            public float Last;
            //public float Median;
            public float Average;

            public static readonly Stats Zero = new Stats
            {
                Last = 0.0f,
                Average = 0.0f,
                //Median = 0.0f
            };
        }

        // resets timing data
        public void Reset()
        {
            WaitForActive();

            sum = 0.0f;
            last = 0.0f;
            count = 0;
            //times.Clear();
        }

        /// <summary>
        /// get timer stats
        /// </summary>
        public Stats Get()
        {
            Update();

            //if(times.Count == 0) return Stats.Zero;

            return new Stats
            {
                Last = last,
                Average = sum / count,
                //Median = times[times.Count / 2]
            };
        }

        /// <summary>
        /// starts the next timer
        /// </summary>
        public void Start()
        {
            Debug.Assert(current == null);

            // get next free timer
            if (cache.Count > 0)
                current = cache.Pop();
            else current = new GpuTimer();

            current.Start();
        }

        /// <summary>
        /// stops the previously started timer
        /// </summary>
        public void Stop()
        {
            Debug.Assert(current != null);
            if (current == null) return;
            current.Stop();
            activeTimers.Enqueue(current);
            current = null;
        }

        /// <summary>
        /// tests if active queues are finished
        /// </summary>
        private void Update()
        {
            while (activeTimers.Count != 0 && activeTimers.Peek().HasData())
            {
                // add data
                var t = activeTimers.Dequeue();
                float dt = t.GetDelta();
                ++count;
                sum += dt;
                last = dt;
                // insert into sorted list
                //var index = times.BinarySearch(dt);
                //if (index < 0) index = ~index;
                //times.Insert(index, dt);

                cache.Push(t);
            }
        }

        private void WaitForActive()
        {
            while (activeTimers.Count != 0)
            {
                activeTimers.Peek().GetDelta();
                cache.Push(activeTimers.Dequeue());
            }
        }

        public void Dispose()
        {
            foreach (var t in activeTimers)
            {
                t.Dispose();
            }

            foreach (var t in cache)
            {
                t.Dispose();
            }

            current?.Dispose();
        }
    }
}
