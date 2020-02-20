using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageViewer.Views.Dialog
{
    /// <summary>
    /// Interaction logic for ShaderExceptionDialog.xaml
    /// </summary>
    public partial class ShaderExceptionDialog : Window
    {
        public ShaderExceptionDialog(string error, string source)
        {
            InitializeComponent();
            this.ErrorTextBox.Text = error;
            Editor.Text = source;

            // find position to scroll to => the first error line is usually after the first bracket  "bla bla (23, 12-15)"
            var idx = error.IndexOf('(');
            if (idx < 0) return;
            // followed by a comma
            var commaIdx = error.IndexOf(',', idx);
            if (commaIdx < 0) return;
            var numString = error.Substring(idx + 1, commaIdx - idx - 1);
            if (!int.TryParse(numString, out var lineNum)) return;

            Editor.ScrollToVerticalOffset((Editor.FontSize + 1.4) * lineNum);
            
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            System.Media.SystemSounds.Hand.Play();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
