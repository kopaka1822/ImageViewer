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
        public bool HasAlpha => Min.Alpha < 1.0f || Max.Alpha > 1.0f;

        public static readonly StatisticsModel Zero = new StatisticsModel{Avg = DefaultStatistics.Zero, Max = DefaultStatistics.Zero, Min = DefaultStatistics.Zero};

        public void Plus(StatisticsModel other)
        {
            Avg.Plus(other.Avg);
            Min.Plus(other.Min);
            Max.Plus(other.Max);
        }

        public void Divide(float value)
        {
            Avg.Divide(value);
            Min.Divide(value);
            Max.Divide(value);
        }
    }
}
