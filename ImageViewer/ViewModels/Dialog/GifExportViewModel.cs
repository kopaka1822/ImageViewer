using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModels.Dialog
{
    public class GifExportViewModel
    {
        public int FramesPerSecond => SelectedFps == 0 ? 30 : 60;

        public int SelectedFps { get; set; } = 1;

        public int TotalSeconds { get; set; } = 3;

        public int SliderSize { get; set; } = 3;
    }
}
