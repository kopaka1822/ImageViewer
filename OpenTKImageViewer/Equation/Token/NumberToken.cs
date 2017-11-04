using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Token
{
    class NumberToken : ValueToken
    {
        private readonly float value;

        public NumberToken(float number)
        {
            value = number;
        }

        public override string ToOpenGl()
        {
            return $"vec4(float({value}))";
        }
    }
}
