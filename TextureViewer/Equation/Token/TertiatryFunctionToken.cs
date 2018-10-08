using System;

namespace TextureViewer.Equation.Token
{
    public class TertiatryFunctionToken : ValueToken
    {
        private readonly string funcName;
        private readonly ValueToken value1;
        private readonly ValueToken value2;
        private readonly ValueToken value3;

        public TertiatryFunctionToken(string funcName, ValueToken value1, ValueToken value2, ValueToken value3)
        {
            this.funcName = funcName;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
        }

        public override string ToOpenGl()
        {
            if (funcName.ToLower() == "rgb")
            {
                // transform values into rgb vector
                return $"vec4(({value1.ToOpenGl()}).r, " +
                       $"({value2.ToOpenGl()}).r, " +
                       $"({value3.ToOpenGl()}).r, 1.0)";
            }

            throw new Exception("invalid string as function name: " + funcName);
        }
    }
}
