using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Statistics
{
    /// <summary>
    /// statistics about an image
    /// </summary>
    public class StatisticsModel
    {
        public ColorSpace Avg { get; }
        public ColorSpace Min { get; }
        public ColorSpace Max { get; }

        /// <summary>
        /// image has an active alpha channel if at least one pixel has alpha unequal to one
        /// </summary>
        public bool HasAlpha => Min.Linear.Alpha < 1.0f;
    }
}
