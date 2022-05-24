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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageViewer.Models;
using ImageViewer.ViewModels.Display;
using ImageViewer.Models.Display;

namespace ImageViewer.Views.Display
{
    /// <summary>
    /// Interaction logic for MovieView.xaml
    /// </summary>
    public partial class MovieView : UserControl, IDisposable
    {
        private readonly MovieViewModel viewModel;

        public MovieView(ModelsEx models, MovieDisplayModel baseModel)
        {
            InitializeComponent();
            DataContext = viewModel = new MovieViewModel(models, baseModel);
        }

        public void Dispose()
        {
            viewModel?.Dispose();
        }
    }
}
