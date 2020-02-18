using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Scaling.Down
{
    internal class TriangleScalingShader : DownscalingShaderBase
    {
        // left part: 1 + x => integral from -1 to x: 0.5x² + x + 0.5
        // right part: 1 - x => integral from 0 to x: x - 0.5x²
        public TriangleScalingShader(int stretch = 2) : base("return x < 0.0 ? 0.5 + x * (1 + 0.5 * x) : 0.5 + x * (1.0 - 0.5 * x);", stretch)
        {
        }
    }
}
