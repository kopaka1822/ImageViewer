namespace ImageFramework.Model.Equation.Token
{
    public class Token
    {
        public enum Type
        {
            Value,
            Operation1,
            Operation2,
            Operation3,
            BracketOpen,
            BracketClose,
            Seperator,
            Function
        }

        public Type TokenType { get; }

        protected Token(Type type)
        {
            TokenType = type;
        }
    }
}
