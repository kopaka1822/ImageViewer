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
using TextureViewer.ImageView;

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for LayerWindow.xaml
    /// </summary>
    public partial class LayerWindow : Window, IUniqueDialog
    {
        class ModeBoxItem : ComboBoxItem
        {
            public ImageViewType ViewType { get; set; }
        }

        public bool IsClosing { get; set; }
        private readonly App parent;
        private MainWindow activeWindow;

        public LayerWindow(App parent)
        {
            this.parent = parent;
            IsClosing = false;
            InitializeComponent();
        }

        private void LayerWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.CloseDialog(App.UniqueDialog.Layer);
        }

        public void UpdateContent(MainWindow window)
        {
            if (!ReferenceEquals(window, activeWindow))
            {
                if (activeWindow != null)
                    activeWindow.Context.ChangedLayer -= OnChangedLayer;
                if (window != null)
                    window.Context.ChangedLayer += OnChangedLayer;
            }

            activeWindow = window;
            LayerList.Items.Clear();

            if (window != null)
            {
                // set layer
                var activeLayer = window.Context.ActiveLayer;
                foreach (var item in window.GenerateLayerItems())
                    LayerList.Items.Add(item);

                window.Context.ActiveLayer = activeLayer;
                LayerList.SelectedIndex = (int) window.Context.ActiveLayer;

                // set combo box
                BoxMode.Items.Clear();
                int selectedBox = 0;
                var activeView = window.CurrentView;
                int currentView = 0;
                foreach (var view in window.GetAvailableViews())
                {
                    BoxMode.Items.Add(new ModeBoxItem{Content = view.ToString(), ViewType = view});
                    if (activeView == view)
                        selectedBox = currentView;
                    ++currentView;
                }
                BoxMode.SelectedIndex = selectedBox;
            }
        }

        private void OnChangedLayer(object sender, EventArgs e)
        {
            LayerList.SelectedIndex = (int)activeWindow.Context.ActiveLayer;
        }

        private void LayerList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activeWindow == null)
                return;
            activeWindow.Context.ActiveLayer = (uint) LayerList.SelectedIndex;
        }

        private void BoxMode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activeWindow == null)
                return;
            if (BoxMode.SelectedIndex >= 0)
            {
                activeWindow.CurrentView = ((ModeBoxItem) BoxMode.Items.GetItemAt(BoxMode.SelectedIndex)).ViewType;
            }
        }
    }
}
