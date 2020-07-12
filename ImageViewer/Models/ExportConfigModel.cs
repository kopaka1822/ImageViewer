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
using ImageViewer.UtilityEx;

namespace ImageViewer.Models
{
    public class ExportConfigModel : CropManager
    {
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
    }
}
