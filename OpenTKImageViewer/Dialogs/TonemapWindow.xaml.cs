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
    public partial class TonemapWindow : Window, IUniqueDialog
    {
        public bool IsClosing { get; set; } = false;
        private readonly App parent;
        private MainWindow activeWindow;

        public TonemapWindow(App parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void TonemapWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.CloseDialog(App.UniqueDialog.Tonemap);
        }


        public void UpdateContent(MainWindow window)
        {
            // TODO
            activeWindow = window;
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = false;

            if (ofd.ShowDialog() != true) return;

            // load shader
            activeWindow.EnableOpenGl();
            try
            {
                var param = activeWindow.Context.Tonemapper.LoadShader(ofd.FileName);
                activeWindow.Context.Tonemapper.Apply(new List<ToneParameter>{param});
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
