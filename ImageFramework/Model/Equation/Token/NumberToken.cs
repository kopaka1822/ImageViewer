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
            var num = $"float({value.ToString(Models.Culture)})";
            return $"float4({num}, {num}, {num}, {num})";
        }
    }
}
