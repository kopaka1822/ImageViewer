using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation
{
    public class SingleCharToken : Token
    {
        private char symbol;
        public SingleCharToken(Type type, char symbol) : base(type)
        {
            this.symbol = symbol;
        }
    }
}
