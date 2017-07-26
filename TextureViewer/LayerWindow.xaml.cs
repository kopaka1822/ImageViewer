using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for LayerWindow.xaml
    /// </summary>
    public partial class LayerWindow : Window
    {
        public bool IsClosing { get; private set; }
        private App parent;

        public LayerWindow(App parent)
        {
            this.parent = parent;
            IsClosing = false;
            InitializeComponent();
        }

        private void LayerWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.CloseLayerWindow();
        }

        public void UpdateContent(MainWindow window)
        {
            LayerList.Items.Clear();
            foreach (var item in window.GenerateLayerItems())
                LayerList.Items.Add(item);
        }
    }
}
