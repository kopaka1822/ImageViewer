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
        private MainWindow activeWindow;

        public ImageWindow(App parent)
        {
            this.parent = parent;
            IsClosing = false;
            InitializeComponent();
            EquationBox1.FontFamily = new FontFamily("Consolas");
        }

        public void UpdateContent(MainWindow window)
        {
            if (!ReferenceEquals(window, activeWindow))
            {
                if (activeWindow != null)
                    activeWindow.Context.ChangedImages -= OnChangedImages;
                if (window != null)
                    window.Context.ChangedImages += OnChangedImages;
            }

            activeWindow = window;
            RefreshImageList();
        }

        private void OnChangedImages(object sender, EventArgs e)
        {
            RefreshImageList();
        }

        private void RefreshImageList()
        {
            ImageList.Items.Clear();
            if (activeWindow != null)
            {
                // refresh image list
                foreach (var item in activeWindow.GenerateImageItems())
                    ImageList.Items.Add(item);

                EquationBox1.Text = activeWindow.Context.ImageFormula1.Original;
            }
        }

        private void ImageWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.CloseDialog(App.UniqueDialog.Image);
        }

        private void ButtonApply_OnClick(object sender, RoutedEventArgs e)
        {
            if (activeWindow == null) return;
            try
            {
                activeWindow.Context.ImageFormula1.ApplyFormula(EquationBox1.Text, activeWindow.Context.GetNumImages());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
