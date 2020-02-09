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
    public class ImageVarianceStats
    {
        public ImageVarianceStats(ITexture luma, ITexture expected, ITexture variance)
        {
            Luma = luma;
            Expected = expected;
            Variance = variance;
        }

        public ITexture Luma { get; }
        public ITexture Expected { get; }
        public ITexture Variance { get; }
    }
}
