using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTKImageViewer.UI;

namespace OpenTKImageViewer.View
{
    public class EmptyView : IImageView
    {
        public void Update(MainWindow window)
        {

            window.StatusBar.LayerMode = StatusBarControl.LayerModeType.None;
        }

        public void Draw()
        {
            
        }

        public void OnDrag(Vector diff, MainWindow window)
        {
            
        }

        public void OnScroll(double diff, Point mouse)
        {
            
        }

        public void UpdateMouseDisplay(MainWindow window)
        {
            window.StatusBar.SetMouseCoordinates(0, 0);
        }

        public void SetZoom(float dec)
        {
            
        }
    }
}
