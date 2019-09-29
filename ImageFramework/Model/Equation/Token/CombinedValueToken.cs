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

        public override string ToOpenGl()
        {
            if (operat.Symbol == '^')
            {
                return $"pow({left.ToOpenGl()},{right.ToOpenGl()})";
            }
            // + - * / is easy
            return $"({left.ToOpenGl()} {operat.Symbol} {right.ToOpenGl()})";
        }
    }
}
