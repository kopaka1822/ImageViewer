using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class EquationsViewModel
    {
        private readonly EquationViewModel[] viewModels;

        public EquationsViewModel(ImageEquationsModel model)
        {
            viewModels = new EquationViewModel[model.NumEquations];
            for (var i = 0; i < viewModels.Length; ++i)
            {
                viewModels[i] = new EquationViewModel(model.Get(i));
            }
        }

        public EquationViewModel Equation1 => viewModels[0];
        public EquationViewModel Equation2 => viewModels[1];
    }
}
