using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Equation.Token
{
    internal class IntrinsicToken : ValueToken
    {
        private readonly string funcName;

        public IntrinsicToken(string funcName)
        {
            this.funcName = funcName;
        }

        public override string ToHlsl()
        {
            // aqcuire opengl function name
            if (!GetIntrinsicFunction(funcName, out var res))
                throw new Exception("invalid string as function name: " + funcName);
            return res;
        }

        private static bool GetIntrinsicFunction(string name, out string res)
        {
            switch (name)
            {
                // image viewer extensions
                case "pos":
                    res = "float4(fcoord, 1.0)";
                    break;
                case "cpos":
                    res = "float4(fcoord * 2.0 - 1.0, 1.0)";
                    break;
                case "ipos":
                    res = "float4(coord, 1.0)";
                    break;
                case "size":
                    res = "float4(width, height, depth, 1.0)";
                    break;
                case "layer":
                    res = "f4(layer)";
                    break;
                default:
                    res = null;
                    return false;
            }
            return true;
        }
    }
}
