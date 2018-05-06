using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models
{
    /// <summary>
    /// container for the image equations
    /// </summary>
    public class ImageEquationsModel
    {
        private readonly ImageEquationModel[] equations = new ImageEquationModel[2]
        {
            new ImageEquationModel(true), new ImageEquationModel(false) 
        };

        public int NumEquations => equations.Length;

        public ImageEquationModel Get(int id)
        {
            Debug.Assert(id >= 0 && id < NumEquations);
            return equations[id];
        }
    }
}
