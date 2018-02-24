using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Token
{
    class BinaryFunctionToken : ValueToken
    {
        private readonly string funcName;
        private readonly ValueToken value1;
        private readonly ValueToken value2;

        public BinaryFunctionToken(string funcName, ValueToken value1, ValueToken value2)
        {
            this.funcName = funcName;
            this.value1 = value1;
            this.value2 = value2;
        }

        public override string ToOpenGl()
        {
            if(!isOpenGlFunction())
                throw new Exception("invalid string as function name: " + funcName);

            return funcName + "(" + value1.ToOpenGl() + "," + value2.ToOpenGl() + ")";
        }

        private bool isOpenGlFunction()
        {
            switch (funcName)
            {
                case "min":
                case "max":
                case "atan":
                case "pow":
                case "mod":
                case "step":
                    return true;
            }
            return false;
        }
    }
}
