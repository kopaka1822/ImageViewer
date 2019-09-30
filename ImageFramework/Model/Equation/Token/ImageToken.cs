namespace ImageFramework.Model.Equation.Token
{
    internal class ImageToken : ValueToken
    {
        public readonly int Id;

        public ImageToken(int id)
        {
            this.Id = id;
        }

        public override string ToHlsl()
        {
            return $"GetTexture{Id.ToString(Models.Culture)}()";
        }
    }
}
