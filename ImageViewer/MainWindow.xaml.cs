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
using ImageFramework.DirectX;
using ImageViewer.DirectX;

namespace ImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SwapChain chain = null;

        public MainWindow()
        {
            InitializeComponent();


        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var adapter = new SwapChainAdapter(BorderHost);
            BorderHost.Child = adapter;
            chain = adapter.SwapChain;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            chain.BeginFrame();

            chain.EndFrame();
        }
    }
}
