using System;
using System.Diagnostics;
using System.Globalization;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model.Statistics
{
    /// <summary>
    /// statistics about an image
    /// </summary>
    public class StatisticsModel : IDisposable
    {
        internal GpuBuffer buffer = null;
        private StatisticsShader luminanceShader;
        private StatisticsShader uniformShader;
        private StatisticsShader lumaShader;
        private StatisticsShader lightnessShader;
        private StatisticsShader alphaShader;
        private StatisticsShader alphaCoverageShader;
        private ReduceShader minReduce;
        private ReduceShader maxReduce;
        private ReduceShader avgReduce;

        private SharedModel shared;

        public StatisticsModel(SharedModel shared)
        {
            this.shared = shared;
        }

        internal StatisticsShader LuminanceShader => luminanceShader ?? (luminanceShader = new StatisticsShader(shared.Upload, StatisticsShader.LuminanceValue));
        internal StatisticsShader UniformShader => uniformShader ?? (uniformShader = new StatisticsShader(shared.Upload, StatisticsShader.UniformWeightValue));
        internal StatisticsShader LumaShader => lumaShader ?? (lumaShader = new StatisticsShader(shared.Upload, StatisticsShader.LumaValue));
        internal StatisticsShader LightnessShader => lightnessShader ?? (lightnessShader = new StatisticsShader(shared.Upload, StatisticsShader.LightnessValue));
        internal StatisticsShader AlphaShader => alphaShader ?? (alphaShader = new StatisticsShader(shared.Upload, StatisticsShader.AlphaValue));

        internal StatisticsShader AlphaCoverageShader => alphaCoverageShader ?? (alphaCoverageShader = new StatisticsShader(shared.Upload, StatisticsShader.AlphaTestCoverage));

        internal ReduceShader MinReduce => minReduce ?? (minReduce = new ReduceShader(shared.Upload, "min(a,b)", float.MaxValue.ToString(Models.Culture)));
        internal ReduceShader MaxReduce => maxReduce ?? (maxReduce = new ReduceShader(shared.Upload, "max(a,b)", float.MinValue.ToString(Models.Culture)));
        internal ReduceShader AvgReduce => avgReduce ?? (avgReduce = new ReduceShader(shared.Upload, "a+b", "0.0"));
        //public bool HasAlpha => !(Min.Alpha == 1.0f && Max.Alpha == 1.0f);

        internal float GetStats(ITexture texture, LayerMipmapRange lm, StatisticsShader statShader, ReduceShader redShader, bool normalize)
        {
            Debug.Assert(lm.IsSingleMipmap);

            // obtain a buffer that is big enough
            int numElements = texture.Size.GetMip(lm.FirstMipmap).Product;
            if(lm.AllLayer) numElements *= texture.LayerMipmap.Layers;

            // allocate buffer that is big enough
            GetBuffer(numElements);

            // copy all values into buffer
            statShader.CopyToBuffer(texture, buffer, lm);

            // execute reduce
            redShader.Run(buffer, numElements);

            shared.Download.CopyFrom(buffer, sizeof(float));

            var res = shared.Download.GetData<float>();
            if (normalize) res /= numElements;
            return res;
        }

        /// <summary>
        /// allocates a buffer that is big enough to hold numElement floats
        /// </summary>
        /// <param name="numElements"></param>
        /// <returns></returns>
        internal GpuBuffer GetBuffer(int numElements)
        {
            if (buffer == null || buffer.ElementCount < numElements)
            {
                buffer?.Dispose();
                buffer = new GpuBuffer(4, numElements);
            }

            return buffer;
        }

        public DefaultStatistics GetStatisticsFor(ITexture texture, LayerMipmapRange lm)
        {
            return new DefaultStatistics(this, texture, lm);
        }

        public DefaultStatistics GetStatisticsFor(ITexture texture)
        {
            return new DefaultStatistics(this, texture, LayerMipmapRange.MostDetailed);
        }

        public AlphaStatistics GetAlphaStatisticsFor(ITexture texture, float threshold, LayerMipmapRange lm)
        {
            // set user parameter for alpha coverage statistics
            AlphaCoverageShader.UserParameter = threshold;

            return new AlphaStatistics
            {
                Threshold = threshold,
                Coverage = GetStats(texture, lm, AlphaCoverageShader, AvgReduce, true)
            };
        }

        public void Dispose()
        {
            buffer?.Dispose();
            luminanceShader?.Dispose();
            uniformShader?.Dispose();
            lumaShader?.Dispose();
            lightnessShader?.Dispose();
            alphaShader?.Dispose();
            alphaCoverageShader?.Dispose();
            minReduce?.Dispose();
            maxReduce?.Dispose();
            avgReduce?.Dispose();
        }
    }
}
