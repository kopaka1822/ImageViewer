using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TextureViewer.Annotations;

namespace TextureViewer.Models
{
    public class DisplayModel : INotifyPropertyChanged
    {
        public enum GrayscaleMode
        {
            Disabled,
            Red,
            Green,
            Blue,
            Alpha
        }

        public enum ViewMode
        {
            Empty,
            Single,
            CubeMap,
            Polar,
            CubeCrossView
        }

        private readonly ImagesModel imagesModel;

        private ViewMode activeView = ViewMode.Empty;
        public ViewMode ActiveView
        {
            get => activeView;
            set
            {
                if (value == activeView) return;
                activeView = value;
                OnPropertyChanged(nameof(ActiveView));
            }
        }

        private float zoom = 1.0f;
        public float Zoom
        {
            get => zoom;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == zoom) return;
                zoom = value;
                OnPropertyChanged(nameof(Zoom));
            }
        }

        private Matrix4 aspectRatio = Matrix4.Identity;
        public Matrix4 AspectRatio
        {
            get => aspectRatio;
            set
            {
                if (aspectRatio.Equals(value)) return;
                aspectRatio = value;
                OnPropertyChanged(nameof(AspectRatio));
            }
        }

        public DisplayModel(ImagesModel imagesModel)
        {
            this.imagesModel = imagesModel;
            this.imagesModel.PropertyChanged += ImagesModelOnPropertyChanged;
        }

        private void ImagesModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumMipmaps):
                    // reset active mipmap
                    ActiveMipmap = 0;
                    break;
                case nameof(ImagesModel.NumLayers):
                    // reset active layer
                    ActiveLayer = 0;
                    break;
            }
        }

        private int activeLayer = 0;
        public int ActiveLayer
        {
            get => activeLayer;
            set
            {
                Debug.Assert(value == 0 || (value >= 0 && value < imagesModel.NumLayers));
                if (value == activeLayer) return;
                activeLayer = value;
                OnPropertyChanged(nameof(ActiveLayer));
            }
        }

        private int activeMipmap = 0;
        public int ActiveMipmap
        {
            get => activeMipmap;
            set
            {
                Debug.Assert(value == 0 || (value >= 0 && value < imagesModel.NumMipmaps));
                if (value == activeMipmap) return;
                activeMipmap = value;
                OnPropertyChanged(nameof(ActiveMipmap));
            }
        }

        private bool linearInterpolation = false;
        public bool LinearInterpolation
        {
            get => linearInterpolation;
            set
            {
                if (value == linearInterpolation) return;
                linearInterpolation = value;
                OnPropertyChanged(nameof(LinearInterpolation));
            }
        }

        private GrayscaleMode grayscale = GrayscaleMode.Disabled;
        public GrayscaleMode Grayscale
        {
            get => grayscale;
            set
            {
                if (value == grayscale) return;
                grayscale = value;
                OnPropertyChanged(nameof(Grayscale));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
