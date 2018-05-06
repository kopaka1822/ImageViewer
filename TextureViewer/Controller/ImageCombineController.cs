using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Models;

namespace TextureViewer.Controller
{
    public class ImageCombineController
    {
        private readonly ImageEquationModel equation;

        public ImageCombineController(ImageEquationModel equation)
        {
            this.equation = equation;
            equation.ColorFormula.PropertyChanged += FormulaOnPropertyChanged;
            equation.ColorFormula.PropertyChanged += FormulaOnPropertyChanged;
            equation.PropertyChanged += EquationOnPropertyChanged;
        }

        private void EquationOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImageEquationModel.UseFilter):
                    RecomputeFilter();
                    break;
            }
        }

        private void FormulaOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName.Equals(nameof(FormulaModel.Converted)))
                RecomputeCombineShader();
        }

        private void RecomputeCombineShader()
        {

        }

        private void RecomputeFilter()
        {

        }
    }
}
