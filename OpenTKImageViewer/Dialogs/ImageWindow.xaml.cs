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

namespace OpenTKImageViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window, IUniqueDialog
    {
        private readonly App parent;
        public bool IsClosing { get; set; }

        public ImageWindow(App parent)
        {
            this.parent = parent;
            IsClosing = false;
            InitializeComponent();
        }

        public void UpdateContent(MainWindow window)
        {
            ImageList.Items.Clear();
            foreach (var item in window.GenerateImageItems())
                ImageList.Items.Add(item);
        }

        private void ImageWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.CloseDialog(App.UniqueDialog.Image);
        }
    }
}
