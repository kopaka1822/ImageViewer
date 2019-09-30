using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;

namespace ImageFramework.Model.Equation
{
    public class FormulaModel : INotifyPropertyChanged
    {
        private readonly ImagesModel images;
        public FormulaModel(ImagesModel images, int defaultId)
        {
            Debug.Assert(defaultId >= 0);

            this.images = images;
            this.formula = $"I{defaultId}";
            this.Converted = $"GetTexture{defaultId}()";
            this.isValid = defaultId < images.NumImages;

            images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    if(images.PrevNumImages > images.NumImages)
                    {
                        // image count decreased, formula still valid?
                        if (IsValid)
                        {
                            IsValid = TestFormulaValid();
                        }
                    }
                    else // image count increased, if the formula was invalid, is it valid now?
                    {
                        if (!IsValid)
                        {
                            IsValid = TestFormulaValid();
                        }
                    }
                    break;
            }
        }

        // the formula which is displayed
        private string formula;

        public string Formula
        {
            get => formula;
            set
            {
                if(value == null || formula.Equals(value)) return;

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

        private bool isValid = false;
        public bool IsValid
        {
            get => isValid;
            private set
            {
                if (value == isValid) return;
                isValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }

        /// <summary>
        /// tests if the given formula is valid
        /// </summary>
        /// <param name="f">formula to test</param>
        /// <returns>null if valid, error string if invalid</returns>
        public string TestFormula(string f)
        {
            try
            {
                var eq = new Equation(f, Math.Max(images.NumImages, 1));
                eq.GetHlslExpression();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return null;
        }

        private bool TestFormulaValid()
        {
            return TestFormula(Formula) == null;
        }

        /// <summary>
        /// convertes the formula into an hlsl expression.
        /// </summary>
        /// <param name="f">image formula</param>
        /// <returns>hlsl expression</returns>
        /// <exception cref="Exception">on conversion failure</exception>
        private string ConvertFormula(string f)
        {
            var eq = new Equation(f, Math.Max(images.NumImages, 1));
            FirstImageId = eq.GetFirstImageId();
            return eq.GetHlslExpression();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
