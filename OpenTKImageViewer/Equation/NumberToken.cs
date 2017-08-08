using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation
{
    class NumberToken : Token
    {
        private float value;

        public NumberToken(float number) : 
            base(Type.Value)
        {
            value = number;
        }
    }
}
