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

namespace OpenTKImageViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
            this.Topmost = true;
        }

        public void SetProgress(double percent)
        {
            percent = Math.Max(0.0, Math.Min(1.0, percent));
            ProgressBar.Value = percent * 100.0f;
        }

        public void SetDescription(string text)
        {
            TextDescription.Text = text;
        }
    }
}
