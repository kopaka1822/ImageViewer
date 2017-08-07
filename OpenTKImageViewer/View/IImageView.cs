using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenTKImageViewer.View
{
    public enum ImageViewType
    {
        Empty,
        Single,
        CubeMap,
        Polar
    }

    interface IImageView
    {
        void Update();
        void Draw();
        void OnDrag(Vector diff);
        void OnScroll(double diff, Point mouse);
    }
}
