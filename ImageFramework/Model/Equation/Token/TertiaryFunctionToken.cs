using System;

namespace ImageFramework.Model.Equation.Token
{
    public class TertiatryFunctionToken : ValueToken
    {
        private readonly string funcName;
        private readonly ValueToken value1;
        private readonly ValueToken value2;
        private readonly ValueToken value3;

        public TertiatryFunctionToken(string funcName, ValueToken value1, ValueToken value2, ValueToken value3)
        {
            this.funcName = funcName.ToLower();
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
        }

        public override string ToHlsl()
        {
            switch (funcName)
            {
                case "rgb":
                    // transform values into rgb vector
                    return $"float4(({value1.ToHlsl()}).x, " +
                           $"({value2.ToHlsl()}).x, " +
                           $"({value3.ToHlsl()}).x, 1.0)";
                case "lerp":
                    return $"lerp({value1.ToHlsl()}, {value2.ToHlsl()}, ({value3.ToHlsl()}).x)";
                case "clamp":
                    return $"clamp({value1.ToHlsl()}, {value2.ToHlsl()}, {value3.ToHlsl()})";
            }
            throw new Exception("invalid string as function name: " + funcName);
        }

        public override float ToFloat()
        {
            switch (funcName)
            {
                case "min": return Math.Min(value1.ToFloat(), Math.Min(value2.ToFloat(), value3.ToFloat()));
                case "max": return Math.Max(value1.ToFloat(), Math.Max(value2.ToFloat(), value3.ToFloat()));
            }

            throw new Exception("invalid string as function name: " + funcName);
        }

        public override string ToString()
        {
            return $"{funcName}({value1.ToString()}, {value2.ToString()}, {value3.ToString()})";
        }
    }
}
