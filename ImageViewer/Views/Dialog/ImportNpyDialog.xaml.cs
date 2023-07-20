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
    /// Interaction logic for ImportNpyDialog.xaml
    /// </summary>
    public partial class ImportNpyDialog : Window
    {
        public ImportNpyDialog(ImportNpyViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
