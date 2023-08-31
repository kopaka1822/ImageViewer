namespace ImageFramework.Model.Equation.Token
{
    public class SingleCharToken : Token
    {
        public char Symbol { get; }
        public SingleCharToken(Type type, char symbol) : base(type)
        {
            this.Symbol = symbol;
        }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}
