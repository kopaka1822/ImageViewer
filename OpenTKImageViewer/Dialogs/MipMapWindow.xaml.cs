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
    /// Interaction logic for MipMapWindow.xaml
    /// </summary>
    public partial class MipMapWindow : Window, IUniqueDialog
    {
        private App parent;
        private MainWindow activeWindow;

        public MipMapWindow(App parent)
        {
            this.parent = parent;
            IsClosing = false;
            InitializeComponent();
        }

        private void MipMapWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.CloseDialog(App.UniqueDialog.Mipmaps);
        }

        public bool IsClosing { get; set; }

        public void UpdateContent(MainWindow window)
        {
            if (!ReferenceEquals(window, activeWindow))
            {
                if (activeWindow != null)
                    activeWindow.Context.ChangedMipmap -= OnChangedMipmap;
                if (window != null)
                    window.Context.ChangedMipmap += OnChangedMipmap;
            }

            activeWindow = window;
            MipMapList.Items.Clear();

            if (window != null)
            {
                var activeMipmap = window.Context.ActiveMipmap;
                foreach (var item in window.GenerateMipMapItems())
                    MipMapList.Items.Add(item);

                window.Context.ActiveMipmap = activeMipmap;
                MipMapList.SelectedIndex = (int)activeWindow.Context.ActiveMipmap;
            }
        }

        private void OnChangedMipmap(object sender, EventArgs e)
        {
            MipMapList.SelectedIndex = (int)activeWindow.Context.ActiveMipmap;
        }

        private void MipMapList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activeWindow == null)
                return;
            activeWindow.Context.ActiveMipmap = (uint)MipMapList.SelectedIndex;
            activeWindow.RedrawFrame();
        }
    }
}
