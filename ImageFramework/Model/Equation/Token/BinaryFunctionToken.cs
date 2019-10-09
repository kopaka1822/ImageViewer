using System;
using System.Windows;

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

            var val1 = value1.ToHlsl();
            var val2 = value2.ToHlsl();

            if (funcName == "pow")
                val1 = $"max({val1}, 0.0)"; // do this to suppress warning and undefined behaviour

            if (convertToRgb)
                return "float4(f3(" + funcName + "((" + val1 + ").xyz,(" + val2 + ").xyz)), 1.0)";

            return funcName + "(" + val1 + "," + val2 + ")";
        }

        private bool IsHlslFunction()
        {
            switch (funcName)
            {
                case "min":
                case "max":
                case "atan2":
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
