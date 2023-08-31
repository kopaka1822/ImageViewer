using System;

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
                // use extended pow
                return $"powEx({left.ToHlsl()},{right.ToHlsl()})";
            }
            // + - * / is easy
            return $"({left.ToHlsl()} {operat.Symbol} {right.ToHlsl()})";
        }

        public override float ToFloat()
        {
            if (operat.Symbol == '^')
            {
                return (float) Math.Pow(left.ToFloat(), right.ToFloat());
            }

            // + - * / is easy
            switch (operat.Symbol)
            {
                case '+': return left.ToFloat() + right.ToFloat();
                case '-': return left.ToFloat() - right.ToFloat();
                case '*': return left.ToFloat() * right.ToFloat();
                case '/': return left.ToFloat() / right.ToFloat();
            }
            throw new Exception("unknown operator: " + operat.Symbol);
        }

        public override string ToString()
        {
            return $"({left.ToString()} {operat.Symbol} {right.ToString()})";
        }
    }
}
