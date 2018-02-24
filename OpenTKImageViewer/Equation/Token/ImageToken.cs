using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation.Token
{
    class ImageToken : ValueToken
    {
        private int id;

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
