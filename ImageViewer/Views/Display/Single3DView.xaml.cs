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

namespace ImageViewer.Views.Display
{
    /// <summary>
    /// Interaction logic for Single3DView.xaml
    /// </summary>
    public partial class Single3DView : UserControl, IDisposable
    {
        private readonly Single3DDisplayViewModel vm;
        public Single3DView(ModelsEx models)
        {
            InitializeComponent();
            
            DataContext = vm = new Single3DDisplayViewModel(models);
        }

        public void Dispose()
        {
            vm.Dispose();
        }
    }
}
