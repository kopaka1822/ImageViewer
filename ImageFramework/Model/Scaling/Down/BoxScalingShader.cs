using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Scaling.Down
{
    internal class BoxScalingShader : DownscalingShaderBase
    {
        // base function: 1
        // integral: x * 0.5 + 0.5 (area = 1)
        public BoxScalingShader() : base("return x * 0.5 + 0.5;", 1)
        {
        }
    }
}
