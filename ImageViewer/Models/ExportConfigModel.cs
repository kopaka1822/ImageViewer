using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Utility;

namespace ImageViewer.Models
{
    public class ExportConfigModel : INotifyPropertyChanged
    {
        private Float3 cropStartf = Float3.One;

        /// <summary>
        /// crop start in relative coordinates [0, 1]
        /// CropStart.ToPixel is the first included pixel
        /// </summary>
        public Float3 CropStart
        {
            get => cropStartf;
            set
            {
                Debug.Assert((value >= Float3.Zero).AllTrue());
                Debug.Assert((value <= Float3.One).AllTrue());
                if(value == cropStartf) return;
                cropStartf = value;
                OnPropertyChanged(nameof(CropStart));
            }
        }

        private Float3 cropEndf = Float3.One;

        /// <summary>
        /// crop end in relative coordinates [0, 1]
        /// CropEnd.ToPixel is the last included pixel.
        /// CropStart == CropEnd => exactly one pixel will be exported
        /// </summary>
        public Float3 CropEnd
        {
            get => cropEndf;
            set
            {
                Debug.Assert((value >= Float3.Zero).AllTrue());
                Debug.Assert((value <= Float3.One).AllTrue());
                if(value == cropEndf) return;
                cropEndf = value;
                OnPropertyChanged(nameof(CropEnd));
            }
        }

        private bool useCropping = false;

        public bool UseCropping
        {
            get => useCropping;
            set
            {
                if(value == useCropping) return;
                useCropping = value;
                OnPropertyChanged(nameof(UseCropping));
            }
        }

        private int layer = -1;

        public int Layer
        {
            get => layer;
            set
            {
                if(layer == value) return;
                layer = value;
                OnPropertyChanged(nameof(Layer));
            }
        }

        private int mipmap = -1;

        public int Mipmap
        {
            get => mipmap;
            set
            {
                if(mipmap == value) return;
                mipmap = value;
                OnPropertyChanged(nameof(Mipmap));
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
