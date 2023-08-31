using System;

namespace ImageFramework.Model.Equation.Token
{
    internal class ImageToken : ValueToken
    {
        public int Id;

        public ImageToken(int id)
        {
            this.Id = id;
        }

        public override string ToHlsl()
        {
            return $"GetTexture{Id.ToString(Models.Culture)}(coord)";
        }

        public override float ToFloat()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"I{Id}";
        }
    }
}
