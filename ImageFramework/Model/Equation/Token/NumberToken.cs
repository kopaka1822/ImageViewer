namespace ImageFramework.Model.Equation.Token
{
    internal class NumberToken : ValueToken
    {
        private readonly float value;

        public NumberToken(float number)
        {
            value = number;
        }

        public override string ToHlsl()
        {
            return ToHlsl(value);
        }

        // for unit testing purposes
        internal static string ToHlsl(float value)
        {
            return $"f4({value.ToString(Models.Culture)})";
        }
    }
}
