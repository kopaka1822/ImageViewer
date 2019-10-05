using ImageFramework.Model.Shader;

namespace ImageFramework.Model
{
    /// <summary>
    /// statistics about an image
    /// </summary>
    public class StatisticsModel
    {
        public DefaultStatistics Avg { get; internal set; }
        public DefaultStatistics Min { get; internal set; }
        public DefaultStatistics Max { get; internal set; }

        /// <summary>
        /// image has an active alpha channel if at least one pixel has alpha unequal to one
        /// </summary>
        public bool HasAlpha => Min.Alpha < 1.0f;
    }
}
