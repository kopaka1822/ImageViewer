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

        public override string ToHlsl()
        {
            // aqcuire opengl function name
            if (!GetHlslFunction(funcName, out var start, out var end))
                throw new Exception("invalid string as function name: " + funcName);
            return start + value.ToHlsl() + end;
        }

        public static string GetUnaryHelperFunctions()
        {
            return @"
float4 srgbAsSnorm(float4 v)
{
    // undo srgb (linear color space values back to srgb space)
    float4 res = toSrgb(v);
    // only convert rgb    
    [unroll] for(int i = 0; i < 3; ++i) {
        // get byte value
        int byte = int(res[i] * 255.0 + 0.5);
        
        // according to dx spec. 127 maps to 1.0, -127 (129) and -128 (128) maps to -1.0
        if(byte <= 127)
            res[i] = byte / 127.0;
        else
        {
            byte |= 0xFFFFFF00; // extend sign
            //res[i] = -(1.0 - min((byte - 127) / 127.0, 1.0));
            res[i] = max(byte / 127.0, -1.0);
        }
    }
    return res;
}
";
        }

        private static bool GetHlslFunction(string name, out string front, out string end)
        {
            switch (name)
            {
                // image viewer extensions
                case "alpha":
                    front = "(";
                    end = ").aaaa";
                    break;
                case "red":
                    front = "(";
                    end = ").rrrr";
                    break;
                case "green":
                    front = "(";
                    end = ").gggg";
                    break;
                case "blue":
                    front = "(";
                    end = ").bbbb";
                    break;
                // image viewer conversion extensions
                case "tosrgb": // linear to srgb
                case "srgbasunorm": // reinterpret srgb image as unorm
                    front = "toSrgb(";
                    end = ")";
                    break;
                case "fromsrgb": // srgb to linear
                    front = "fromSrgb(";
                    end = ")";
                    break;
                case "srgbassnorm": // reinterpret srgb image as snorm
                    front = "srgbAsSnorm(";
                    end = ")";
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
                case "exp2":
                case "sign":
                case "floor":
                case "ceil":
                case "frac":
                case "trunc":
                    front = name + "(";
                    end = ")";
                    break;

                case "log":
                case "log2":
                case "log10":
                case "sqrt":
                    front = name + "Ex(";
                    end = ")";
                    break;

                case "normalize":
                    front = "float4(normalizeEx((";
                    end = ").xyz), 1.0)";
                    break;
                case "length":
                    front = "f4(length((";
                    end = ").xyz))";
                    break;
                case "all":
                case "any":
                    front = $"f4({name}(";
                    end = ")?1.0:0.0)";
                    break;
                case "radians":
                    front = $"f4({name}((";
                    end = ").x))";
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
