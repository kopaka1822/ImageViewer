using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;

namespace ImageViewer.ViewModels.Dialog
{
    public class SimpleImageViewModel : INotifyPropertyChanged
    {
        public int ImageWidth { get; set; } = 1024;
        public int ImageHeight { get; set; } = 1024;
        public int ImageDepth { get; set; } = 1;

        public int MaxImageWidth => Device.MAX_TEXTURE_2D_DIMENSION;
        public int MaxImageHeight => Device.MAX_TEXTURE_2D_DIMENSION;
        public int MaxImageDepth => Device.MAX_TEXTURE_3D_DIMENSION;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
