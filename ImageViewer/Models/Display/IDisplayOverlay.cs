using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageFramework.Utility;

namespace ImageViewer.Models.Display
{
    public interface IDisplayOverlay : IDisposable
    {
        void MouseMove(Size3 texel);
        void MouseClick(MouseButton button, bool down, Size3 texel);
    }
}
