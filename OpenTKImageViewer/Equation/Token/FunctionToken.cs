using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Token
{
    public class FunctionToken : Token
    {
        public readonly string FuncName;

        public FunctionToken(string name) : 
            base(Type.Function)
        {
            this.FuncName = name;
        }
    }
}
