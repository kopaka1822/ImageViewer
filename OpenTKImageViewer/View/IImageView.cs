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
        void Update(MainWindow window);
        void Draw();
        void OnDrag(Vector diff, MainWindow window);
        void OnScroll(double diff, Point mouse);
        void UpdateMouseDisplay(MainWindow window);
    }
}
