using System;

namespace ImageFramework.Model.Equation.Token
{
    internal class BinaryFunctionToken : ValueToken
    {
        private string funcName;
        private readonly ValueToken value1;
        private readonly ValueToken value2;
        private bool convertToRgb = false;

        public BinaryFunctionToken(string funcName, ValueToken value1, ValueToken value2)
        {
            this.funcName = funcName.ToLower();
            this.value1 = value1;
            this.value2 = value2;
        }

        public override string ToHlsl()
        {
            if (!IsHlslFunction())
                throw new Exception("invalid string as function name: " + funcName);

            if (convertToRgb)
                return "f4(f3(" + funcName + "(f3(" + value1.ToHlsl() + "),f3(" + value2.ToHlsl() + "))), 1.0)";
            return funcName + "(" + value1.ToHlsl() + "," + value2.ToHlsl() + ")";
        }

        private bool IsHlslFunction()
        {
            switch (funcName)
            {
                case "min":
                case "max":
                case "atan":
                case "pow":
                case "fmod":
                case "step":
                    return true;
                case "dot":
                case "cross":
                    convertToRgb = true;
                    return true;

                // not core but provided by the image viewer
                case "equal":
                case "bigger":
                case "smaller":
                case "smallereq":
                case "biggereq":
                    // add f because functions like equal already exist but they return a bvec
                    funcName = "f" + funcName;
                    return true;
            }
            return false;
        }
    }
}
