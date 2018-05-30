using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TextureViewer.Views
{
    public class StatisticTextBox : TextBox
    {
        // prevent interaction with the box
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            SelectionStart = 0;
            SelectionLength = Text.Length;
            Clipboard.SetText(Text);
        }
    }
}
