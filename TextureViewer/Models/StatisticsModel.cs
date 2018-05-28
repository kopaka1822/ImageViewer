using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models
{
    public class StatisticsModel
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
        }

        public ColorSpace Sum { get; }
        public ColorSpace Min { get; }
        public ColorSpace Max { get; }

        public StatisticsModel(ColorSpace sum, ColorSpace min, ColorSpace max)
        {
            Sum = sum;
            Min = min;
            Max = max;
        }

        public static readonly StatisticsModel ZERO = new StatisticsModel(ColorSpace.ZERO, ColorSpace.ZERO, ColorSpace.ZERO);
    }
}
