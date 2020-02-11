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
    internal class VarianceShader : NonSepKernelShader
    {
        public VarianceShader(int radius, float variance) :
            base(radius, new []
            {
                "in_values",
                "in_expected", // expected values
            }, @"
float weightSum = 0.0;
float varSum = 0.0;
float expected = in_expected[texel(id)];
", $@"
int radius2 = dot(id - coord, id - coord); // current squared radius
float w = exp(-0.5 * radius2 / {variance}); // gauss weight
weightSum += w;
float v = in_values[texel(coord)] - expected;
varSum += w * v * v; // gauss weighted variance
", @"
out_image[texel(id, layer)] = varSum / weightSum;
", "float")
        {}

        public void Run(ITexture values, ITexture expectedValues, ITexture dst, LayerMipmapSlice lm,
            UploadBuffer upload)
        {
            Run(new []{values, expectedValues}, dst, lm, upload);
        }
    }
}
