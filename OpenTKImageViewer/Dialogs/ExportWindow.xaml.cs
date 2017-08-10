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

namespace OpenTKImageViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        private MainWindow parent;
        public ExportWindow(MainWindow parent, string filename)
        {
            this.parent = parent;
            InitializeComponent();
            BoxFilename.Text = filename;

            GenerateLayerItems();
            GenerateMipmapItems();
        }

        private void GenerateLayerItems()
        {
            BoxLayer.Items.Clear();
            for (int i = 0; i < parent.Context.GetNumLayers(); ++i)
            {
                BoxLayer.Items.Add(new ComboBoxItem { Content = "Layer " + i });
            }
            BoxLayer.SelectedIndex = (int)parent.Context.ActiveLayer;
        }

        private void GenerateMipmapItems()
        {
            BoxMipmaps.Items.Clear();
            for (int i = 0; i < parent.Context.GetNumMipmaps(); ++i)
            {
                BoxMipmaps.Items.Add(new ComboBoxItem
                {
                    Content = "Mipmap " + i + " - "
                              + parent.Context.GetWidth(i) + "x" + parent.Context.GetHeight(i)
                });
            }
            BoxMipmaps.SelectedIndex = (int)parent.Context.ActiveMipmap;
        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClock(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
