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
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void ButtonApply_OnClock(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
