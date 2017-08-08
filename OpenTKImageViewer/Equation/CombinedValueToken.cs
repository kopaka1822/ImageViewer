using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation
{
    class CombinedValueToken : Token
    {
        private Token left;
        // operator between the two tokens
        private Token operat;
        private Token right;

        public CombinedValueToken(Token left, Token operat, Token right) : 
            base(Type.Value)
        {
            this.left = left;
            this.operat = operat;
            this.right = right;
        }
    }
}
