using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation
{
    public class SingleCharToken : Token
    {
        public char Symbol { get; private set; }
        public SingleCharToken(Type type, char symbol) : base(type)
        {
            this.Symbol = symbol;
        }
    }
}
