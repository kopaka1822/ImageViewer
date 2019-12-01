using ImageFramework.Model.Shader;

namespace ImageFramework.Model
{
    /// <summary>
    /// statistics about an image
    /// </summary>
    public class StatisticsModel
    {
        public DefaultStatistics Avg;
        public DefaultStatistics Min;
        public DefaultStatistics Max;

        /// <summary>
        /// image has an active alpha channel if at least one pixel has alpha unequal to one
        /// </summary>
        public bool HasAlpha => !(Min.Alpha == 1.0f && Max.Alpha == 1.0f);

        public static readonly StatisticsModel Zero = new StatisticsModel
        {
            Avg = DefaultStatistics.Zero,
            Max = DefaultStatistics.Zero,
            Min = DefaultStatistics.Zero
        };

        public static readonly StatisticsModel Init = new StatisticsModel
        {
            Avg = DefaultStatistics.Zero,
            Max = DefaultStatistics.MinStats,
            Min = DefaultStatistics.MaxStats
        };

        public void Plus(StatisticsModel other)
        {
            Avg.Plus(other.Avg);
            Min.Min(other.Min);
            Max.Max(other.Max);
        }

        public void Divide(float value)
        {
            Avg.Divide(value);
        }
    }
}
