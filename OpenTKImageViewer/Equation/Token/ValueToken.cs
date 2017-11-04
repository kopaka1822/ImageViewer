using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Token
{
    public abstract class ValueToken : Token
    {
        protected ValueToken() : base(Type.Value)
        {
        }

        public abstract string ToOpenGl();
    }
}
