using System;

namespace ImageFramework.Model.Equation.Token
{
    internal class UnaryFunctionToken : ValueToken
    {
        private readonly string funcName;
        private readonly ValueToken value;

        public UnaryFunctionToken(string funcName, ValueToken value)
        {
            this.funcName = funcName.ToLower();
            this.value = value;
        }

        public override string ToOpenGl()
        {
            // aqcuire opengl function name
            if (!GetOpenglFunction(funcName, out var start, out var end))
                throw new Exception("invalid string as function name: " + funcName);
            return start + value.ToOpenGl() + end;
        }

        private static bool GetOpenglFunction(string name, out string front, out string end)
        {
            switch (name)
            {
                case "alpha":
                    front = "vec4((";
                    end = ").a)";
                    break;
                case "tosrgb":
                    front = "toSrgb(";
                    end = ")";
                    break;
                case "fromsrgb":
                    front = "fromSrgb(";
                    end = ")";
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
                    front = name + "(";
                    end = ")";
                    break;
                case "normalize":
                    front = "vec4(" + name + "(vec3(";
                    end = ")), 1.0)";
                    break;
                case "length":
                    front = "vec4(length(vec3(";
                    end = ")))";
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
