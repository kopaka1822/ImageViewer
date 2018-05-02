using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using TextureViewer.Annotations;

namespace TextureViewer.Models
{
    public class ViewModeModel : INotifyPropertyChanged
    {
        public enum ViewMode
        {
            Empty,
            Single,
            CubeMap,
            Polar,
            CubeCrossView
        }

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

        private Vector2 mousePosition = new Vector2(0.0f, 0.0f);
        public Vector2 MousePosition
        {
            get => mousePosition;
            set
            {
                if (mousePosition.Equals(value)) return;
                mousePosition = value;
                OnPropertyChanged(nameof(MousePosition));
            }
        }

        private Matrix4 aspectRatio = Matrix4.Identity;
        public Matrix4 AspectRation
        {
            get => aspectRatio;
            set
            {
                if (aspectRatio.Equals(value)) return;
                aspectRatio = value;
                OnPropertyChanged(nameof(AspectRation));
            }
        }

        /// <summary>
        /// transforms mouse coordinates from range [-1, 1] to [0, imageSize] and clamps the range if input exceeds [-1, 1]
        /// </summary>
        /// <param name="transMouse"> [-1, 1]</param>
        /// <param name="imageWidth">width of the current image</param>
        /// <param name="imageHeight">height of the current image</param>
        /// <returns>[0, 1]</returns>
        public static Vector4 MouseToTextureCoordinates(Vector4 transMouse, int imageWidth, int imageHeight)
        {
            // trans mouse is betweem [-1,1] in texture coordinates => to [0,1]
            transMouse.X += 1.0f;
            transMouse.X /= 2.0f;

            transMouse.Y += 1.0f;
            transMouse.Y /= 2.0f;

            // clamp value
            transMouse.X = Math.Min(0.9999f, Math.Max(0.0f, transMouse.X));
            transMouse.Y = Math.Min(0.9999f, Math.Max(0.0f, transMouse.Y));

            // scale with mipmap level
            transMouse.X *= (float)imageWidth;
            transMouse.Y *= (float)imageHeight;

            return transMouse;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
