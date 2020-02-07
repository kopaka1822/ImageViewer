using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
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
            Alpha,
            Size // placeholder
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
                case Types.Alpha:
                    return Get(Alpha, metric);
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

        // gets a suitability rating for the format based on the statistic values
        public int GetFormatRating(GliFormat format)
        {
            const int alphaMissWeight = 1000; // penalty if alpha channel is required but missing
            const int alphaMatchWeight = 5; // bonus for same alpha state
            const int signWeight = 10;
            const int unormWeight = 300;

            // determine basic properties of image
            bool hasAlpha = HasAlpha;
            bool isUnormed = Average.Min >= 0.0f && Average.Max <= 1.0f
                && Alpha.Min >= 0.0f && Alpha.Max <= 1.0f;
            bool isSigned = Average.Min < 0.0f;

            int rating = 0;
            // is alpha required?
            if (hasAlpha && !format.HasAlpha())
                rating -= alphaMissWeight; // alpha should be present if image needs alpha
            else if (hasAlpha == format.HasAlpha())
                rating += alphaMatchWeight; // alpha state matching

            // is a signed format required?
            var pixelType = format.GetDataType();
            if (isSigned && !pixelType.IsSigned())
                rating -= signWeight; // signed format is required
            else if (isSigned == pixelType.IsSigned())
                rating += signWeight;

            // range 0 1?
            if (!isUnormed && pixelType == PixelDataType.UNorm) // unorm is not sufficient
                rating -= unormWeight;
            else if (!isUnormed && pixelType == PixelDataType.Srgb) // maybe tonemapping to srgb? better than unorm probably
                rating -= unormWeight / 10;

            //else if (isUnormed == pixelType.IsUnormed())
            //    rating += unormWeight;

            return rating;
        }

        /// gets suitability rating for format and takes preferred format into account
        public int GetFormatRating(GliFormat format, GliFormat preferredFormat)
        {
            var rating = GetFormatRating(format);
            var preferredPixelType = preferredFormat.GetDataType();
            var pixelType = format.GetDataType();
            bool isSrgb = preferredPixelType == PixelDataType.Srgb;
            bool hasRgb = preferredFormat.HasRgb();

            if (format == preferredFormat)
                rating += 150;

            // keep srgb specifier
            if (isSrgb == (pixelType == PixelDataType.Srgb))
                rating += 200;
            else
            {
                // prefer srgb formats over unorm etc. when converting from hdr to ldr
                if (!preferredFormat.IsAtMost8bit() && !preferredPixelType.IsUnormed() && pixelType == PixelDataType.Srgb)
                    rating -= 50;
                else
                    rating -= 200;
            }

            // small bonus for same datatype
            if (preferredPixelType == pixelType)
                rating += 5;

            // small bonus for rgb match
            if (hasRgb == format.HasRgb())
                rating += 15;

            // small bonus for high precision
            if (!preferredFormat.IsLessThan8Bit() && !format.IsLessThan8Bit())
                rating += 20;

            // small bonus for kept compression
            if (preferredFormat.IsCompressed() && format.IsCompressed())
                rating += 10;

            // try to keep hdr formats
            if (!preferredFormat.IsAtMost8bit() && !format.IsAtMost8bit())
            {
                // keep unorm property
                if(preferredPixelType.IsUnormed() == pixelType.IsUnormed())
                    rating += 50;
            }
                




            return rating;
        }
    }
}
