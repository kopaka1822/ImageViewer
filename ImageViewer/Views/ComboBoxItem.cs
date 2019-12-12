using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ImageViewer.Views
{
    public class ComboBoxItem<T> : TextBlock
    {
        public T Cargo { get; }

        public ComboBoxItem(string name, T cargo, string tooltip = null)
        {
            Cargo = cargo;

            Text = name;
            if (!string.IsNullOrEmpty(tooltip))
                ToolTip = tooltip;

            Foreground = GetForeground();
        }

        private static SolidColorBrush foregroundBrush = null;
        static SolidColorBrush GetForeground()
        {
            if (foregroundBrush == null)
            {
                foregroundBrush = (SolidColorBrush)App.Current.FindResource("FontBrush");
            }

            return foregroundBrush;
        }
    }
}
