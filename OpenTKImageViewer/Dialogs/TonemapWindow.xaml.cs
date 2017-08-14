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
using Microsoft.Win32;
using OpenTKImageViewer.Tonemapping;

namespace OpenTKImageViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for TonemapWindow.xaml
    /// </summary>
    public partial class TonemapWindow : Window
    {
        public bool IsClosing { get; set; } = false;
        private MainWindow parent;

        public TonemapWindow(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void TonemapWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.TonemapDialog = null;
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = false;

            if (ofd.ShowDialog() != true) return;

            // load shader
            parent.EnableOpenGl();
            try
            {
                var param = parent.Context.Tonemapper.LoadShader(ofd.FileName);
                parent.Context.Tonemapper.Apply(new List<ToneParameter>{param});
                // TODO add to list
            }
            catch (Exception exception)
            {
                App.ShowErrorDialog(this, exception.Message);
            }
        }

        private void ButtonApply_OnClock(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
