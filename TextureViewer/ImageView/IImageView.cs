using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpGL;

namespace TextureViewer.ImageView
{
    public enum ImageViewType
    {
        Empty,
        Single,
        CubeMap
    }

    interface IImageView
    {
        void Init(OpenGL gl, MainWindow parent);
        void Draw();
        void OnDrag(Vector diff);
        void OnScroll(double diff, Point mouse);
    }
}
