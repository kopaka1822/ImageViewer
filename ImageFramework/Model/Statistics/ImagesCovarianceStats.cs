using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;

namespace ImageFramework.Model.Statistics
{
    /// <summary>
    /// expected value, variance and covariance of two images
    /// </summary>
    public class ImagesCovarianceStats
    {
        public ImagesCovarianceStats(ImageVarianceStats image1, ImageVarianceStats image2, ITexture covariance)
        {
            Image1 = image1;
            Image2 = image2;
            Covariance = covariance;
        }

        public ImageVarianceStats Image1 { get; }
        public ImageVarianceStats Image2 { get; }
        public ITexture Covariance { get; }
    }
}
