using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.ImageLoader;
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

        private GliFormat? format = null;
        // last format that was used for exporting
        public GliFormat? Format
        {
            get => format;
            set
            {
                if (value == null || value == format) return;
                format = value;
                OnPropertyChanged(nameof(Format));
            }
        }

        // path manager for intermediate directories etc.
        public PathManager Path { get; } = new PathManager();
    }
}
