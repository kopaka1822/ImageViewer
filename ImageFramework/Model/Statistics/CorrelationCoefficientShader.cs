using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model.Statistics
{
    internal class CorrelationCoefficientShader : NonSepKernelShader
    {
        public CorrelationCoefficientShader(int radius, float variance) :
            base(radius, new[]
            {
                "in_values1",
                "in_values2",
                "in_expected1", // expected values
                "in_expected2"
            }, @"
float weightSum = 0.0;
float coSum = 0.0;
float expected1 = in_expected1[texel(id)];
float expected2 = in_expected2[texel(id)];
", $@"
int radius2 = dot(id - coord, id - coord); // current squared radius
float w = exp(-0.5 * radius2 / {variance}); // gauss weight
weightSum += w;
coSum += w * (in_values1[texel(coord)] - expected1) * (in_values2[texel(coord)] - expected2); // gauss weighted variance
", @"
out_image[texel(id, layer)] = coSum / weightSum;
", "float")
        { }

        public void Run(ITexture values1, ITexture values2, ITexture expected1, ITexture expected2, ITexture dst, LayerMipmapSlice lm,
            UploadBuffer upload)
        {
            Run(new[] { values1, values2, expected1, expected2 }, dst, lm, upload);
        }
    }
}
