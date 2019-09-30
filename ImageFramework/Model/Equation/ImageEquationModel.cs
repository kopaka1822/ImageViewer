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
        private readonly ImagesModel images;

        public ImageEquationModel(ImagesModel images, int defaultImage)
        {
            this.images = images;
            Color = new FormulaModel(images, defaultImage);
            Alpha = new FormulaModel(images, defaultImage);
            Color.PropertyChanged += FormulaOnPropertyChanged;
            Alpha.PropertyChanged += FormulaOnPropertyChanged;
        }

        private void FormulaOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(FormulaModel.IsValid))
                OnPropertyChanged(nameof(IsValid));
        }

        public FormulaModel Color { get; }
        public FormulaModel Alpha { get; }

        public bool IsValid => Color.IsValid && Alpha.IsValid;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
