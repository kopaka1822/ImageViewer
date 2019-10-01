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
        public FormulaModel(int defaultId)
        {
            Debug.Assert(defaultId >= 0);

            this.formula = $"I{defaultId}";
            this.Converted = $"GetTexture{defaultId}()";
            this.FirstImageId = defaultId;
            this.MaxImageId = defaultId;
        }

        // the formula which is displayed
        private string formula;

        public string Formula
        {
            get => formula;
            set
            {
                if(value == null || formula.Equals(value)) return;

                var oldFirst = FirstImageId;
                var oldMax = MaxImageId;

                var converted = ConvertFormula(value);
                var changed = !converted.Equals(Converted);

                // does it result in the same conversion?
                Converted = converted;
                formula = value;
                if(changed)
                    OnPropertyChanged(nameof(Converted));
                if(oldFirst != FirstImageId)
                    OnPropertyChanged(nameof(FirstImageId));
                if (oldMax != MaxImageId)
                    OnPropertyChanged(nameof(MaxImageId));
                OnPropertyChanged(nameof(Formula));
            }
        }

        // the id of the first image that was used in the equation
        public int FirstImageId { get; private set; }

        // the highest image id that was used in the equation
        public int MaxImageId { get; private set; }

        // the converted formula
        public string Converted { get; private set; } 

        /// <summary>
        /// tests if the given formula is valid
        /// </summary>
        /// <param name="f">formula to test</param>
        /// <returns>null if valid, error string if invalid</returns>
        public string TestFormula(string f)
        {
            try
            {
                var eq = new Equation(f);
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
            var eq = new Equation(f);
            var res = eq.GetHlslExpression();
            FirstImageId = eq.FirstImageId;
            MaxImageId = eq.MaxImageId;
            return res;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
