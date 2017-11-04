using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Token
{
    class UnaryFunctionToken : ValueToken
    {
        private readonly string funcName;
        private readonly ValueToken value;

        public UnaryFunctionToken(string funcName, ValueToken value)
        {
            this.funcName = funcName;
            this.value = value;
        }

        public override string ToOpenGl()
        {
            // aqcuire opengl function name
            string start;
            string end;
            if(!GetOpenglFunction(funcName, out start, out end))
                throw new Exception("invalid string as function name: " + funcName);
            return start + value.ToOpenGl() + end;
        }

        private bool GetOpenglFunction(string name, out string front, out string end)
        {
            switch (name)
            {
                case "alpha":
                    front = "vec4((";
                    end = ").a)";
                    break;
                case "red":
                    front = "vec4((";
                    end = ").r)";
                    break;
                case "green":
                    front = "vec4((";
                    end = ").g)";
                    break;
                case "blue":
                    front = "vec4((";
                    end = ").b)";
                    break;
                    // original glsl functions
                case "abs":
                case "sin":
                case "cos":
                case "tan":
                case "asin":
                case "acos":
                case "atan":
                case "exp":
                case "log":
                case "exp2":
                case "log2":
                case "sqrt":
                case "sign":
                case "floor":
                case "ceil":
                case "fract":
                case "normalize":
                    front = name + "(";
                    end = ")";
                    break;
                case "length":
                    front = "vec4(length(";
                    end = "))";
                    break;
                default:
                    front = null;
                    end = null;
                    return false;
            }
            return true;
        }
    }
}
