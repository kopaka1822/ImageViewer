using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Token
{
    public class Token
    {
        public enum Type
        {
            Value,
            Operation1,
            Operation2,
            Operation3,
            BracketOpen,
            BracketClose,
            Function
        }

        public Type TokenType { get; private set; }

        protected Token(Type type)
        {
            TokenType = type;
        }
    }
}
