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
    /// Interaction logic for MipMapWindow.xaml
    /// </summary>
    public partial class MipMapWindow : Window
    {
        private App parent;
        public bool IsClosing { get; private set; }

        public MipMapWindow(App parent)
        {
            this.parent = parent;
            IsClosing = false;
            InitializeComponent();
        }

        private void MipMapWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.CloseMipMapWindow();
        }
    }
}
