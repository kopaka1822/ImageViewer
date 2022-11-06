using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Statistics
{
    /// <summary>
    /// wrapper that contains all relevant statistics for the alpha test with a specific coverage
    /// </summary>
    public struct AlphaStatistics
    {
        // (input) alpha threshold
        public float Threshold { get; set; }
        // (output) percentage of pixels that pass the alpha test
        public float Coverage { get; set; }
    }
}
