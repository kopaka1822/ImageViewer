using System;
using System.Diagnostics;
using ImageFramework.Utility;

namespace ImageFramework.Model.Shader
{
    public struct DefaultStatistics
    {
        public enum Values
        {
            Luminance,
            Luma,
            Lightness,
            Alpha
        }

        // luminance: http://homepages.inf.ed.ac.uk/rbf/CVonline/LOCAL_COPIES/POYNTON1/ColorFAQ.html#RTFToC3
        public float Luminance;
        // luma: http://homepages.inf.ed.ac.uk/rbf/CVonline/LOCAL_COPIES/POYNTON1/ColorFAQ.html#RTFToC11
        public float Luma;
        // CIELAB lightness (0-100): http://homepages.inf.ed.ac.uk/rbf/CVonline/LOCAL_COPIES/POYNTON1/ColorFAQ.html#RTFToC4
        public float Lightness;
        public float Alpha;
        public static readonly DefaultStatistics Zero = new DefaultStatistics{Alpha = 0.0f, Luminance = 0.0f, Lightness = 0.0f, Luma = 0.0f};

        public float Get(Values value)
        {
            switch (value)
            {
                case Values.Luminance:
                    return Luminance;
                case Values.Luma:
                    return Luma;
                case Values.Lightness:
                    return Lightness;
                case Values.Alpha:
                    return Alpha;
            }
            Debug.Assert(false);
            return 0.0f;
        }
    }

    internal class DefaultStatisticsShader : StatisticsShader<DefaultStatistics>
    {
        protected override string GetFunctions()
        {
            return Utility.Utility.ToSrgbFunction();
        }

        protected override string GetOneTimeModifyFunction()
        {

            return @"
float4 res;
// linear luminance
res.r = dot(a.rgb, float3(0.2125, 0.7154, 0.0721));
// (video) luma (sRGB). Uses NTSC luminance vector
res.g = dot(toSrgb(a).rgb, float3(0.299, 0.587, 0.114));
// lightness
res.b = max(116.0 * pow(res.r, 1.0 / 3.0) - 16.0, 0.0);
// keep alpha
res.a = a.a;
return res;
";
        }

        protected override DefaultStatistics GetResult(Color color, int nPixels)
        {
            return new DefaultStatistics
            {
                Luminance = color.Red,
                Luma = color.Green,
                Lightness = color.Blue,
                Alpha = color.Alpha
            };
        }
    }

    internal class MinDefaultStatsticsShader : DefaultStatisticsShader
    {
        internal MinDefaultStatsticsShader()
        {
            Init("MinStatisticsShader");
        }
        protected override string GetCombineFunction()
        {
            return "return min(a, b);";
        }
    }

    internal class MaxDefaultStatsticsShader : DefaultStatisticsShader
    {
        internal MaxDefaultStatsticsShader()
        {
            Init("MaxStatisticsShader");
        }
        protected override string GetCombineFunction()
        {
            return "return max(a, b);";
        }
    }

    internal class AvgDefaultStatisticsShader : DefaultStatisticsShader
    {
        internal AvgDefaultStatisticsShader()
        {
            Init("AvgStatisticsShader");
        }
        protected override DefaultStatistics GetResult(Color color, int nPixels)
        {
            // divide through number of pixels to get average
            float invPixel = 1.0f / nPixels;
            color.Red *= invPixel;
            color.Green *= invPixel;
            color.Blue *= invPixel;
            color.Alpha *= invPixel;

            return base.GetResult(color, nPixels);
        }

        protected override string GetCombineFunction()
        {
            return "return a + b;";
        }
    }
}
