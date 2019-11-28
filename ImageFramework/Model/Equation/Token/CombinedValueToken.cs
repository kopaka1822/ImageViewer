namespace ImageFramework.Model.Equation.Token
{
    internal class CombinedValueToken : ValueToken
    {
        private readonly ValueToken left;
        // operator between the two tokens
        private readonly SingleCharToken operat;
        private readonly ValueToken right;

        public CombinedValueToken(Token left, Token operat, Token right)
        {
            this.left = (ValueToken)left;
            this.operat = (SingleCharToken)operat;
            this.right = (ValueToken)right;
        }

        public override string ToHlsl()
        {
            if (operat.Symbol == '^')
            {
                // do max(..) to prevent undefined behaviour + warning
                return $"pow(max({left.ToHlsl()}, 0.0),{right.ToHlsl()})";
            }
            // + - * / is easy
            return $"({left.ToHlsl()} {operat.Symbol} {right.ToHlsl()})";
        }
    }
}
