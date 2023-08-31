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
using ImageViewer.ViewModels.Dialog;

namespace ImageViewer.Views.Dialog
{
    /// <summary>
    /// Interaction logic for ExportBatchDialog.xaml
    /// </summary>
    public partial class ExportBatchDialog : Window
    {
        private readonly ExportBatchViewModel viewModel;
        public ExportBatchDialog(ExportBatchViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();

            DataContext = viewModel;
        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            if (viewModel.IsValid())
            {
                DialogResult = true;
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}
