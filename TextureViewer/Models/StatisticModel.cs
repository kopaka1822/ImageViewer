using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models
{
    public class StatisticModel
    {
        public struct Channel
        {
            public Channel(float luminance, float red, float green, float blue)
            {
                Luminance = luminance;
                Red = red;
                Green = green;
                Blue = blue;
            }

            public float Luminance { get; }
            public float Red { get; }
            public float Green { get; }
            public float Blue { get; }

            public float Get(StatisticsModel.ChannelType channel)
            {
                switch (channel)
                {
                    case StatisticsModel.ChannelType.Red:
                        return Red;
                    case StatisticsModel.ChannelType.Green:
                        return Green;
                    case StatisticsModel.ChannelType.Blue:
                        return Blue;
                    case StatisticsModel.ChannelType.Luminance:
                        return Luminance;
                }

                return 0.0f;
            }

            public static readonly Channel ZERO = new Channel(0.0f, 0.0f, 0.0f, 0.0f);
        }

        public struct ColorSpace
        {
            public ColorSpace(Channel linear, Channel srgb)
            {
                Linear = linear;
                Srgb = srgb;
            }

            public Channel Linear { get; }
            public Channel Srgb { get; }

            public static readonly ColorSpace ZERO = new ColorSpace(Channel.ZERO, Channel.ZERO);

            public Channel Get(StatisticsModel.ColorSpaceType colorSpace)
            {
                switch (colorSpace)
                {
                    case StatisticsModel.ColorSpaceType.Linear:
                        return Linear;
                    case StatisticsModel.ColorSpaceType.Srgb:
                        return Srgb;
                }

                return Channel.ZERO;
            }
        }

        public ColorSpace Avg { get; }
        public ColorSpace Min { get; }
        public ColorSpace Max { get; }

        public StatisticModel(ColorSpace avg, ColorSpace min, ColorSpace max)
        {
            Avg = avg;
            Min = min;
            Max = max;
        }

        public static readonly StatisticModel ZERO = new StatisticModel(ColorSpace.ZERO, ColorSpace.ZERO, ColorSpace.ZERO);
    }
}
