namespace ImageFramework.Model.Equation.Token
{
    public abstract class ValueToken : Token
    {
        protected ValueToken() : base(Type.Value)
        {
        }

        // for hlsl formula conversion
        public abstract string ToHlsl();

        // for direct formula conversion
        public abstract float ToFloat();
    }
}
