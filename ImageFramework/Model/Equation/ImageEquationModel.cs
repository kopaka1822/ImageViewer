using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;

namespace ImageFramework.Model.Equation
{
    public class ImageEquationModel : INotifyPropertyChanged
    {
        public ImageEquationModel(int defaultImage)
        {
            Color = new FormulaModel(defaultImage);
            Alpha = new FormulaModel(defaultImage);
        }

        public FormulaModel Color { get; }
        public FormulaModel Alpha { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
