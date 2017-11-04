using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Token
{
    class CombinedValueToken : ValueToken
    {
        private ValueToken left;
        // operator between the two tokens
        private SingleCharToken operat;
        private ValueToken right;

        public CombinedValueToken(Token left, Token operat, Token right)
        {
            this.left = (ValueToken) left;
            this.operat = (SingleCharToken) operat;
            this.right = (ValueToken) right;
        }

        public override string ToOpenGl()
        {
            if (operat.Symbol == '^')
            {
                return $"pow({left.ToOpenGl()},{right.ToOpenGl()})";
            }
            // + - * / is easy
            return $"({left.ToOpenGl()} {operat.Symbol} {right.ToOpenGl()})";
        }
    }
}
