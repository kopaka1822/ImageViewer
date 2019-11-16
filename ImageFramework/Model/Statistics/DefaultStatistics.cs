using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using SharpDX.DXGI;

namespace ImageFramework.Model.Statistics
{
    /// <summary>
    /// wrapper for a single statistic type with min, max and avg.
    /// Statistics will only be calculated once when the member is accessed
    /// </summary>
    public class DefaultStatisticsType
    {
        private readonly StatisticsModel parent;
        private readonly StatisticsShader shader;
        private readonly ITexture texture;
        private readonly int mipmap;
        private readonly int layer;

        private float? minValue;
        private float? maxValue;
        private float? avgValue;

        public float Min => (float) (minValue ?? (minValue = parent.GetStats(texture, layer, mipmap, shader, parent.MinReduce, false)));
        public float Max => (float) (maxValue ?? (maxValue = parent.GetStats(texture, layer, mipmap, shader, parent.MaxReduce, false)));
        public float Avg => (float) (avgValue ?? (avgValue = parent.GetStats(texture, layer, mipmap, shader, parent.AvgReduce, true)));

        internal DefaultStatisticsType(StatisticsShader shader, ITexture texture, StatisticsModel parent, int layer, int mipmap)
        {
            this.shader = shader;
            this.texture = texture;
            this.parent = parent;
            this.layer = layer;
            this.mipmap = mipmap;
            minValue = null;
            maxValue = null;
            avgValue = null;
        }

        /// <summary>
        /// empty model constructor
        /// </summary>
        internal DefaultStatisticsType()
        {
            minValue = 0.0f;
            maxValue = 0.0f;
            avgValue = 0.0f;
        }
    }

    /// <summary>
    /// wrapper that contains all statistic types for a texture
    /// </summary>
    public class DefaultStatistics
    {
        public enum Types
        {
            Luminance,
            Average,
            Luma,
            Lightness,
        }

        public enum Metrics
        {
            Min,
            Max,
            Avg
        }

        public readonly DefaultStatisticsType Luminance;
        public readonly DefaultStatisticsType Average;
        public readonly DefaultStatisticsType Luma;
        public readonly DefaultStatisticsType Lightness;
        public readonly DefaultStatisticsType Alpha;

        // ReSharper disable twice CompareOfFloatsByEqualityOperator
        public bool HasAlpha => !(Alpha.Min == 1.0f && Alpha.Max == 1.0f);

        public float Get(Types type, Metrics metric)
        {
            switch (type)
            {
                case Types.Luminance:
                    return Get(Luminance, metric);
                case Types.Average:
                    return Get(Average, metric);
                case Types.Luma:
                    return Get(Luma, metric);
                case Types.Lightness:
                    return Get(Lightness, metric);
            }

            return 0.0f;
        }

        private float Get(DefaultStatisticsType type, Metrics metric)
        {
            switch (metric)
            {
                case Metrics.Min:
                    return type.Min;
                case Metrics.Max:
                    return type.Max;
                case Metrics.Avg:
                    return type.Avg;
            }

            return 0.0f;
        }

        internal DefaultStatistics(StatisticsModel parent, ITexture texture, int layer, int mipmap)
        {
            Luminance = new DefaultStatisticsType(parent.LuminanceShader, texture, parent, layer, mipmap);
            Average = new DefaultStatisticsType(parent.UniformShader, texture, parent, layer, mipmap);
            Luma = new DefaultStatisticsType(parent.LumaShader, texture, parent, layer, mipmap);
            Lightness = new DefaultStatisticsType(parent.LightnessShader, texture, parent, layer, mipmap);
            Alpha = new DefaultStatisticsType(parent.AlphaShader, texture, parent, layer, mipmap);
        }

        /// empty model constructor
        internal DefaultStatistics()
        {
            Luminance = new DefaultStatisticsType();
            Average = Luminance;
            Luma = Luminance;
            Lightness = Luminance;
            Alpha = Luminance;
        }

        public static readonly DefaultStatistics Zero = new DefaultStatistics();
    }
}
