using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Scaling.Down
{
    public class BoxScalingShader : DownscalingShaderBase
    {
        public BoxScalingShader() : base(1.0f, "return x * 0.5 + 0.5;")
        {
        }
    }
}
