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
    /// expected value and variance for a texture
    /// </summary>
    internal class ImageVarianceStats
    {
        public ImageVarianceStats(ITextureCache cache)
        {
            Cache = cache;
            Luma = cache.GetTexture();
            Expected = cache.GetTexture();
            Variance = cache.GetTexture();
        }

        public void Dispose()
        {
            Cache.StoreTexture(Luma);
            Cache.StoreTexture(Expected);
            Cache.StoreTexture(Variance);
        }

        public ITextureCache Cache { get; }

        public ITexture Luma { get; }
        public ITexture Expected { get; }
        public ITexture Variance { get; }
    }
}
