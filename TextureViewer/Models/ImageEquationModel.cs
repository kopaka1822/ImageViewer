using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;

namespace TextureViewer.Models
{
    public class ImageEquationModel : INotifyPropertyChanged
    {
        public ImageEquationModel(bool visible)
        {
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
        public bool UseFilter
        {
            get => useFilter;
            set
            {
                if (value == useFilter) return;
                useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
            }
        }

        // TODO add formula


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
