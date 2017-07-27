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
    public partial class MipMapWindow : Window, IUniqueDialog
    {
        private readonly App parent;
        public bool IsClosing { get; set; }

        public MipMapWindow(App parent)
        {
            this.parent = parent;
            IsClosing = false;
            InitializeComponent();
        }

        /// <summary>
        /// Updates content of this window depending on the passed window
        /// </summary>
        /// <param name="window"></param>
        public void UpdateContent(MainWindow window)
        {
            MipMapList.Items.Clear();
            foreach(var item in window.GenerateMipMapItems())
                MipMapList.Items.Add(item);
            MipMapList.SelectedIndex = (int)window.Context.ActiveMipmap;
        }

        private void MipMapWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.CloseDialog(App.UniqueDialog.Mipmaps);
        }

        private void MipMapList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (parent.GetActiveWindow() == null)
                return;
            parent.GetActiveWindow().Context.ActiveMipmap = (uint)MipMapList.SelectedIndex;
        }
    }
}
