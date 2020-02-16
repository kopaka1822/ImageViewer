using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;

namespace ImageFramework.Model.Statistics
{
    /// <summary>
    /// expected value, variance and covariance of two images
    /// </summary>
    internal class ImagesCorrelationStats : IDisposable
    {
        public ITextureCache Cache { get; }

        public ImagesCorrelationStats(ITextureCache cache)
        {
            this.Cache = cache;
            Image1 = new ImageVarianceStats(cache);
            Image2 = new ImageVarianceStats(cache);
            Correlation = cache.GetTexture();
        }

        public ImageVarianceStats Image1 { get; }
        public ImageVarianceStats Image2 { get; }
        public ITexture Correlation { get; }

        public void Dispose()
        {
            Image1.Dispose();
            Image2.Dispose();
            Cache.StoreTexture(Correlation);
        }
    }
}
