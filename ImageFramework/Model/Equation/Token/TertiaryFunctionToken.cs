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
            if (funcName == "rgb")
            {
                // transform values into rgb vector
                return $"float4(({value1.ToHlsl()}).x, " +
                       $"({value2.ToHlsl()}).x, " +
                       $"({value3.ToHlsl()}).x, 1.0)";
            }

            throw new Exception("invalid string as function name: " + funcName);
        }
    }
}
