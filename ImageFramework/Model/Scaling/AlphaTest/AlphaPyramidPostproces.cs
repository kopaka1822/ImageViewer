
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Statistics;
using ImageFramework.Utility;

namespace ImageFramework.Model.Scaling.AlphaTest
{
    internal class AlphaPyramidPostproces : PostprocessBase
    {
        private readonly StatisticsModel stats;

        public AlphaPyramidPostproces(StatisticsModel stats)
        {
            this.stats = stats;
        }

        public override void Run(ITexture uav, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            for (int layer = 0; layer < uav.NumLayers; ++layer)
            {
                // obtain the desired coverage when alpha blending is used
                float desiredCoverage = stats.GetStatisticsFor(uav, new LayerMipmapSlice(layer, 0)).Alpha.Avg;

                // fix alpha values for all mipmaps (including the most detailed)
                for (int mip = 0; mip < uav.NumMipmaps; ++mip)
                {
                    var lm = new LayerMipmapSlice(layer, mip);
                    // determine number of visible pixels
                    int nOpaque = (int)Math.Ceiling(desiredCoverage * uav.Size.GetMip(mip).Product);

                    // obtain all alpha values
                    var alphas = uav.GetPixelAlphas(lm);
                    int a = 3;
                }
            }
        }
    }
}
