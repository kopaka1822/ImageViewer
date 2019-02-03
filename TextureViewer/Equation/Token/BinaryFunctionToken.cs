using System;

namespace TextureViewer.Equation.Token
{
    internal class BinaryFunctionToken : ValueToken
    {
        private readonly string funcName;
        private readonly ValueToken value1;
        private readonly ValueToken value2;
        private bool convertToRgb = false;

        public BinaryFunctionToken(string funcName, ValueToken value1, ValueToken value2)
        {
            this.funcName = funcName;
            this.value1 = value1;
            this.value2 = value2;
        }

        public override string ToOpenGl()
        {
            if(!IsOpenGlFunction())
                throw new Exception("invalid string as function name: " + funcName);

            if(convertToRgb)
                return "vec4(vec3(" + funcName + "(vec3(" + value1.ToOpenGl() + "),vec3(" + value2.ToOpenGl() + "))), 1.0)";
            return funcName + "(" + value1.ToOpenGl() + "," + value2.ToOpenGl() + ")";
        }

        private bool IsOpenGlFunction()
        {
            switch (funcName.ToLower())
            {
                case "min":
                case "max":
                case "atan":
                case "pow":
                case "mod":
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
                    return true;
            }
            return false;
        }
    }
}
