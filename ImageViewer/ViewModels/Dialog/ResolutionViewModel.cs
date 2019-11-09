using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;

namespace ImageViewer.ViewModels.Dialog
{
    public class ResolutionViewModel : INotifyPropertyChanged
    {
        private int width = 1024;
        public int Width {
            get => width;
            set
            {
                if (value == width) return;
                width = value;
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(OutputResolution));
            }
        }

        private readonly int heightDivide;

        /// <param name="heightDivide">height will be divided by this value</param>
        public ResolutionViewModel(int heightDivide)
        {
            this.heightDivide = heightDivide;
        }

        public int Height => Math.Max(Width / heightDivide, 1);

        public string OutputResolution => $"{Width}x{Height}";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
