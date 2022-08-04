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
            scaleAlphaShader = new TransformShader("return float4(value.r, value.g, value.b, clamp(value.a * userParameter, 0.0, 1.0))", "float4");
        }

        public override void Dispose()
        {
            scaleAlphaShader?.Dispose();
            base.Dispose();
        }

        public override void Run(ITexture uav, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            Debug.Assert(hasAlpha);

            for (int layer = 0; layer < uav.NumLayers; ++layer)
            {
                // obtain the desired coverage when alpha blending is used
                float desiredCoverage = stats.GetStatisticsFor(uav, new LayerMipmapSlice(layer, 0)).Alpha.Avg;

                // fix alpha values for all mipmaps (including the most detailed)
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

                    scaleAlphaShader.UserParameter = alphaScale;
                    if(Math.Abs(alphaScale - 1.0f) > 0.001f)
                        scaleAlphaShader.Run(uav, lm, upload);

                    // DEBUG
                    //float finalCoverage = stats.GetAlphaStatisticsFor(uav, threshold, lm).Coverage;
                    //Debug.Assert(Math.Abs(finalCoverage - desiredCoverage) < bestError * 1.1f);
                }
            }
        }
    }
}
