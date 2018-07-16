namespace TextureViewer.Equation.Token
{
    internal class ImageToken : ValueToken
    {
        private readonly int id;

        public ImageToken(int id)
        {
            this.id = id;
        }

        public override string ToOpenGl()
        {
            return $"GetTexture{id.ToString(App.GetCulture())}()";
        }
    }
}
