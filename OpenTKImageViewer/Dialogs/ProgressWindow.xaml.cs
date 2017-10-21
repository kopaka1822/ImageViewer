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
    public delegate void OperationAbortHandler(object sender, EventArgs e);

    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public event OperationAbortHandler Abort;

        public ProgressWindow()
        {
            InitializeComponent();
            this.Topmost = true;
        }

        /// <summary>
        /// sets the progress bar on a range from 0.0 to 1.0
        /// </summary>
        /// <param name="percent">[0,1]</param>
        public void SetProgress(double percent)
        {
            percent = Math.Max(0.0, Math.Min(1.0, percent));
            ProgressBar.Value = percent * 100.0f;
            ProgressBar.InvalidateVisual();
        }

        public void SetDescription(string text)
        {
            TextDescription.Text = text;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if(ProgressBar.Value < 100.0f)
                OnAbort();

            base.OnClosed(e);
        }

        protected virtual void OnAbort()
        {
            Abort?.Invoke(this, EventArgs.Empty);
        }
    }
}
