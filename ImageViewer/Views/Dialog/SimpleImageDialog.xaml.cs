using ImageViewer.ViewModels.Dialog;
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
    /// Interaction logic for SimpleImageDialog.xaml
    /// </summary>
    public partial class SimpleImageDialog : Window
    {
        public SimpleImageDialog(SimpleImageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonCreate_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
