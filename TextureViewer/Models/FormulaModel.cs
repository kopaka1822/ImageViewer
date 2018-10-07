using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using Xceed.Wpf.DataGrid;

namespace TextureViewer.Models
{
    public class FormulaModel : INotifyPropertyChanged
    {
        private readonly ImagesModel images;
        private readonly ImageEquationModel equation;

        // the formula which is displayed
        private string formula;
        public string Formula
        {
            get => formula;
            set
            {
                if (value == null || formula.Equals(value)) return;

                var converted = ConvertFormula(value);
                var changed = !converted.Equals(Converted);

                // does it result in the same conversion?
                Converted = converted;
                formula = value;

                if(changed)
                    OnPropertyChanged(nameof(Converted));
                OnPropertyChanged(nameof(Formula));
            }
        }

        // the id of the first image that was used in the equation
        public int FirstImageId { get; private set; }

        // the converted formula
        public string Converted { get; private set; }

        /// <summary>
        /// tests if the given formula is valid
        /// </summary>
        /// <param name="f">formula to test</param>
        /// <returns></returns>
        public bool IsValid(string f)
        {
            try
            {
                var eq = new Equation.Equation(f, Math.Max(images.NumImages, 1));
                eq.GetOpenGlExpression();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// convertes the formula into an opengl expression.
        /// Throws exception on failure
        /// </summary>
        /// <param name="f">image formula</param>
        /// <returns>opengl expression</returns>
        private string ConvertFormula(string f)
        {
            var eq = new Equation.Equation(f, Math.Max(images.NumImages, 1));
            FirstImageId = eq.GetFirstImageId();
            return eq.GetOpenGlExpression();
        }

        public FormulaModel(int defaultId, ImagesModel images, ImageEquationModel equation)
        {
            this.images = images;
            this.equation = equation;
            this.formula = "I" + defaultId;
            this.Converted = $"GetTexture{defaultId}()";

            this.images.PropertyChanged += ImagesOnPropertyChanged;
            this.equation.PropertyChanged += EquationOnPropertyChanged;
        }

        private void EquationOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImageEquationModel.Visible):
                    // is the formula still valid?
                    if (equation.Visible)
                        ReevaluateFormula();
                    break;
            }
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // equation is deactivated
            if (!equation.Visible) return;

            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    // image count increased
                    if (images.PrevNumImages > images.NumImages)
                        // image count decreased (evaluate formula)
                        ReevaluateFormula();
                    break;
            }
        }

        /// <summary>
        /// checks if the formula is still valid and resets it to "I0" if invalid
        /// </summary>
        private void ReevaluateFormula()
        {
            try
            {
                ConvertFormula(Formula);
            }
            catch (Exception)
            {
                // formula is no longer valid
                Formula = "I0";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
