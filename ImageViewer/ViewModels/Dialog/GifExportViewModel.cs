using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModels.Dialog
{
    public class GifExportViewModel
    {
        public int FramesPerSecond { get; set; } = 30;

        public int TotalSeconds { get; set; } = 6;

        public int SliderSize { get; set; } = 3;
    }
}
