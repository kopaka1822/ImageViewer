using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.glhelper;

namespace TextureViewer.Models
{
    public class FinalImageModel
    {
        public TextureArray2D Texture { get; private set; }

        private ImageEquationModel equation;

        public FinalImageModel(ImageEquationModel equation)
        {
            this.equation = equation;
        }
    }
}
