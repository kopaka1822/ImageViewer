namespace TextureViewer.Equation.Token
{
    internal class NumberToken : ValueToken
    {
        private readonly float value;

        public NumberToken(float number)
        {
            value = number;
        }

        public override string ToOpenGl()
        {
            return $"vec4(float({value.ToString(App.GetCulture())}))";
        }
    }
}
