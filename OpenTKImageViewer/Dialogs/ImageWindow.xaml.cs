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
    public partial class ImageWindow : Window
    {
        private MainWindow parent;

        public ImageWindow(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
            EquationBox1.FontFamily = new FontFamily("Consolas");
            parent.Context.ChangedImages += OnChangedImages;
            RefreshImageList();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            parent.ImageDialog = null;
        }

        private void OnChangedImages(object sender, EventArgs e)
        {
            RefreshImageList();
        }

        private void RefreshImageList()
        {
            ImageList.Items.Clear();
            
            // refresh image list
            foreach (var item in parent.GenerateImageItems())
                ImageList.Items.Add(item);

            EquationBox1.Text = parent.Context.ImageFormula1.Original;
            
        }

        private void ButtonApply_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                parent.Context.ImageFormula1.ApplyFormula(EquationBox1.Text, parent.Context.GetNumImages());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
