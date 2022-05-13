using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageFramework.Utility;

namespace ImageViewer.Models.Display
{
    public interface IDisplayOverlay : IDisposable
    {
        void MouseMove(Size3 texel);
        void MouseClick(MouseButton button, bool down, Size3 texel);

        // return true if the key was handled by the overlay
        bool OnKeyDown(Key key);

        // additional view that is displayed below the menu bar
        UIElement View { get; }
    }
}
