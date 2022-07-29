using ImageFramework.DirectX;
using ImageFramework.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Shader;
using ImageFramework.Model.Statistics;

namespace ImageFramework.Model.Scaling.AlphaTest
{
    internal class AlphaScalePostprocess : PostprocessBase
    {
        private float threshold = 0.5f;

        private readonly StatisticsModel stats;
        private readonly TransformShader scaleAlphaShader;

        public AlphaScalePostprocess(StatisticsModel stats)
        {
            this.stats = stats;
            // TODO add user parameter to transform shader
            scaleAlphaShader = new TransformShader("float4(value.r, value.g, value.b, value.a * userParameter)", "float4");
        }

        public override void Dispose()
        {


            base.Dispose();
        }

        public override void Run(ITexture uav, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            Debug.Assert(hasAlpha);

            for (int layer = 0; layer < uav.NumLayers; ++layer)
            {
                // obtain the desired coverage when alpha blending is used
                float desiredCoverage = stats.GetStatisticsFor(uav, new LayerMipmapSlice(layer, 0)).Alpha.Avg;

                // fix alpha values for all mipmaps (including the most detailled)
                for (int mip = 0; mip < uav.NumMipmaps; ++mip)
                {
                    var lm = new LayerMipmapSlice(layer, mip);
                    // iteratively find the alpha threshold that results in the desired coverage (using bisection)
                    // basic idea from nvidia texture tools: https://github.com/castano/nvidia-texture-tools/blob/master/src/nvimage/FloatImage.cpp (FloatImage::scaleAlphaToCoverage)
                    float minThreshold = 0.0f;
                    float midThreshold = 0.5f;
                    float maxThreshold = 1.0f;

                    float bestThreshold = 0.5f;
                    float bestError = float.MaxValue;

                    for (int i = 0; i < 10; ++i)
                    {
                        float currentCoverage = stats.GetAlphaStatisticsFor(uav, midThreshold, lm).Coverage;
                        float error = Math.Abs(currentCoverage - desiredCoverage);
                        if (error < bestError)
                        {
                            bestThreshold = midThreshold;
                            bestError = error;
                        }

                        if (currentCoverage > desiredCoverage)
                            minThreshold = midThreshold;
                        else if (currentCoverage < desiredCoverage)
                            maxThreshold = midThreshold;
                        else break;

                        midThreshold = (minThreshold + maxThreshold) * 0.5f;
                    }

                    float alphaScale = threshold / bestThreshold;


                    // TODO scale all alpha values by this
                    int a = 3;
                }
            }
        }
    }
}
