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
    /// Interaction logic for VolumeView.xaml
    /// </summary>
    public partial class RayCastingView : UserControl, IDisposable
    {
        private readonly RayCastingDisplayViewModel vm;

        public RayCastingView(ModelsEx models)
        {
            InitializeComponent();

            DataContext = vm = new RayCastingDisplayViewModel(models);
        }

        public void Dispose()
        {
            vm.Dispose();
        }
    }
}
