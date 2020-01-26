using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model.Scaling.Down
{
    internal class LanzosScalingShader : DownscalingShaderBase
    {
        // base function: sin(pi * x * stretch) * sin(pi * x), x in [-1, 1]
        // stretch values in [1, 3]

        // kaiser (alpha=4) ~ lanzos (alpha=1)
        // see: https://www.wolframalpha.com/input/?i=plot+I_0%284*sqrt%281-x%5E2%29%29%2FI_0%284%29%2C+sin%28PI*x%29%2F%28PI*x%29%2C+x%3D0+to+1

        // for: sin(pi * x)^2
        // approximation of the integral from -1 to x: 0.89734 + x - x^3 / 9 
        // integral is very similar to a linear function (box scaling shader)

        // for stretch = 3 (x will still be in [-1, 1]): sinc(pi * x * 3) * sinc(pi * x)
        // aprox. integral: x - 5.48311355 * x^3 + 18.1830303 * x^5
        public LanzosScalingShader(QuadShader quad) : base(@"
float xsq = x * x;
float xtri = xsq * x;
return x - 5.48311355 * xtri + 18.1830303 * xtri * xsq;", 3, quad)
        {
        }
    }
}
