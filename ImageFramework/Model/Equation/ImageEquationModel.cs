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

        public ImageEquationModel(ImagesModel images, bool visible, int defaultImage)
        {
            this.images = images;
            this.visible = visible;
        }

        private bool visible;

        public bool Visible
        {
            get => visible;
            set
            {
                if (value == visible) return;
                visible = value;
                OnPropertyChanged(nameof(Visible));
            }
        }

        private bool useFilter = true;

        public bool UseFiler
        {
            get => useFilter;
            set
            {
                if (value == useFilter) return;
                useFilter = value;
                OnPropertyChanged(nameof(UseFiler));
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
