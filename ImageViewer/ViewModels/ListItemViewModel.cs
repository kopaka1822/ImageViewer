using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModels
{
    // generic template for list view models
    public class ListItemViewModel<T>
    {
        public T Cargo { get; set; }
        public string Name { get; set; }
        public string ToolTip { get; set; }
    }

    // variant without cargo
    public class ListItemViewModel
    {
        public string Name { get; set; }
        public string ToolTip { get; set; }
    }
}
