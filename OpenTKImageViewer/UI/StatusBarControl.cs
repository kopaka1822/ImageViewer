using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OpenTKImageViewer.View;

namespace OpenTKImageViewer.UI
{
    public class StatusBarControl
    {
        public enum LayerModeType
        {
            All,
            Single,
            None
        }

        class ModeBoxItem : ComboBoxItem
        {
            public ImageViewType ViewType { get; set; }
        }

        private readonly MainWindow window;

        private LayerModeType layerMode = LayerModeType.None;
        public LayerModeType LayerMode
        {
            get => layerMode;
            set
            {
                layerMode = value;
                UpdateLayerBox();
                SetLayerDisplay((int)window.Context.ActiveLayer);
            }
        }

        public StatusBarControl(MainWindow window)
        {
            this.window = window;
            UpdateLayerBox();
            OnLayerChange();
            OnMipmapChange();
            UpdateMipmapBox();

            window.Context.ChangedLayer += (sender, args) => OnLayerChange();
            window.Context.ChangedMipmap += (sender, args) => OnMipmapChange();
            // image loading => no  mipmaps to mipmap 0
            window.Context.ChangedImages += (sender, args) => { OnMipmapChange(); UpdateMipmapBox(); };

            window.ComboBoxView.SelectionChanged += ComboBoxViewOnSelectionChanged;
            window.ComboBoxLayer.SelectionChanged += ComboBoxLayerOnSelectionChanged;
            window.ComboBoxMipmap.SelectionChanged += ComboBoxMipmapOnSelectionChanged;
        }

        private void ComboBoxMipmapOnSelectionChanged(object o, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (window.ComboBoxMipmap.SelectedIndex >= 0)
            {
                window.Context.ActiveMipmap = (uint) window.ComboBoxMipmap.SelectedIndex;
            }
        }

        private void ComboBoxLayerOnSelectionChanged(object o, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (window.ComboBoxLayer.SelectedIndex >= 0)
            {
                if (LayerMode == LayerModeType.Single)
                {
                    window.Context.ActiveLayer = (uint)window.ComboBoxLayer.SelectedIndex;
                }
            }
        }

        private void ComboBoxViewOnSelectionChanged(object o, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if(window.ComboBoxView.SelectedIndex >= 0)
                window.CurrentView = ((ModeBoxItem)window.ComboBoxView.Items.GetItemAt(window.ComboBoxView.SelectedIndex)).ViewType;
        }

        public void SetMouseCoordinates(int x, int y)
        {
            window.TextMousePosition.Text = $"{x}, {y}";
        }

        public Point GetCanonicalMouseCoordinates()
        {
            var p = new Point(window.MousePosition.X / window.GetClientWidth(), 1.0f - window.MousePosition.Y / window.GetClientHeight());
            p.X *= 2.0;
            p.X -= 1.0;
            
            p.Y *= 2.0;
            p.Y -= 1.0;
            return p;
        }

        public void UpdateViewBox()
        {
            var activeView = window.CurrentView;
            int selectedBox = 0;
            int currentView = 0;

            window.ComboBoxView.Items.Clear();
            foreach (var availableView in window.GetAvailableViews())
            {
                window.ComboBoxView.Items.Add(new ModeBoxItem
                {
                    ViewType = availableView,
                    Content = availableView.ToString()
                });
                if (activeView == availableView)
                    selectedBox = currentView;
                ++currentView;
            }
            window.ComboBoxView.SelectedIndex = selectedBox;
            window.ComboBoxView.IsEnabled = window.ComboBoxView.Items.Count >= 2;
        }

        private void UpdateLayerBox()
        {
            window.ComboBoxLayer.Items.Clear();
            switch (LayerMode)
            {
                case LayerModeType.All:
                    window.ComboBoxLayer.Items.Add(new ComboBoxItem { Content = "All Layer"});
                    break;
                case LayerModeType.Single:
                    var selectedLayer = window.Context.ActiveLayer;
                    for (int i = 0; i < window.Context.GetNumLayers(); ++i)
                        window.ComboBoxLayer.Items.Add(new ComboBoxItem {Content = "Layer " + i});
                    window.Context.ActiveLayer = selectedLayer;
                    window.ComboBoxLayer.SelectedIndex = (int)selectedLayer;
                    break;
                case LayerModeType.None:
                    window.ComboBoxLayer.Items.Add(new ComboBoxItem {Content = "No Layer"});
                    break;
            }
            window.ComboBoxLayer.IsEnabled = true;
            if (window.ComboBoxLayer.Items.Count == 1)
                window.ComboBoxLayer.SelectedIndex = 0;
            window.ComboBoxLayer.IsEnabled = window.ComboBoxLayer.Items.Count >= 2;
        }

        private void UpdateMipmapBox()
        {
            var box = window.ComboBoxMipmap;
            box.Items.Clear();
            if (window.Context.GetNumMipmaps() == 0)
            {
                box.Items.Add(new ComboBoxItem {Content = "No Mipmap"});
            }
            else
            {
                var activeMipmap = window.Context.ActiveMipmap;
                for (int curMipmap = 0; curMipmap < window.Context.GetNumMipmaps(); ++curMipmap)
                {
                    box.Items.Add(new ComboBoxItem{ Content = window.Context.GetWidth(curMipmap).ToString() + "x" + window.Context.GetHeight(curMipmap).ToString() });
                }
                window.Context.ActiveMipmap = activeMipmap;
                window.ComboBoxMipmap.SelectedIndex = (int)activeMipmap;
            }

            box.IsEnabled = true;
            if (box.Items.Count == 1)
                box.SelectedIndex = 0;
            box.IsEnabled = box.Items.Count >= 2;
        }

        private void SetLayerDisplay(int layer)
        {
            if (LayerMode == LayerModeType.Single)
            {
                window.ComboBoxLayer.SelectedIndex = layer;
            }
        }

        private void SetMipmapDisplay(int mipmap)
        {
            if (window.Context.GetNumMipmaps() > 0)
            {
                window.ComboBoxMipmap.SelectedIndex = mipmap;
            }
        }

        private void OnLayerChange()
        {
            SetLayerDisplay((int)window.Context.ActiveLayer);
        }
        
        private void OnMipmapChange()
        {
            SetMipmapDisplay((int)window.Context.ActiveMipmap);
        }
    }
}
