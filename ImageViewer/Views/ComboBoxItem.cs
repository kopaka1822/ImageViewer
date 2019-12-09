using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ImageViewer.Views
{
    public class ComboBoxItem<T> : TextBlock
    {
        private readonly string name;
        public T Cargo { get; }

        public ComboBoxItem(string name, T cargo, string tooltip = null)
        {
            this.name = name;
            Cargo = cargo;

            Text = this.name;
            if (!string.IsNullOrEmpty(tooltip))
                ToolTip = tooltip;
        }


    }
}
