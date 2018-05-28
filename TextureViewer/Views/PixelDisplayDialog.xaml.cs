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
using TextureViewer.ViewModels.Dialog;

namespace TextureViewer.Views
{
    /// <summary>
    /// Interaction logic for PixelDisplayDialog.xaml
    /// </summary>
    public partial class PixelDisplayDialog : Window
    {
        private readonly PixelDisplayViewModel viewModel;

        public PixelDisplayDialog(Models.Models models)
        {
            viewModel = new PixelDisplayViewModel(models);
            DataContext = viewModel;
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            // unregister model callbacks
            viewModel.Unregister();

            base.OnClosed(e);
        }

        private void Apply_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.Apply();
            DialogResult = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
