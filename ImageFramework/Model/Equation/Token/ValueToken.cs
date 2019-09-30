namespace ImageFramework.Model.Equation.Token
{
    public abstract class ValueToken : Token
    {
        protected ValueToken() : base(Type.Value)
        {
        }

        public abstract string ToHlsl();
    }
}
