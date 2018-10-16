namespace TextureViewer.Equation.Token
{
    internal class ImageToken : ValueToken
    {
        public readonly int Id;

        public ImageToken(int id)
        {
            this.Id = id;
        }

        public override string ToOpenGl()
        {
            return $"GetTexture{Id.ToString(App.GetCulture())}()";
        }
    }
}
