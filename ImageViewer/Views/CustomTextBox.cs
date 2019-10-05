using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ImageViewer.Views
{
    /// <summary>
    /// text box that updates bindings when enter is pressed
    /// </summary>
    public class CustomTextBox : TextBox
    {
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                base.OnKeyUp(e);
                // prevent the key event from bubbling up
                e.Handled = true;
                return;
            }

            // update binding if enter was pressed
            e.Handled = true;
            var binding = BindingOperations.GetBindingExpression(this, TextProperty);
            binding?.UpdateSource();
            Keyboard.ClearFocus();
        }
    }
}
