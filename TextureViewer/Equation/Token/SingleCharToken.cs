namespace TextureViewer.Equation.Token
{
    public class SingleCharToken : Token
    {
        public char Symbol { get; }
        public SingleCharToken(Type type, char symbol) : base(type)
        {
            this.Symbol = symbol;
        }
    }
}
