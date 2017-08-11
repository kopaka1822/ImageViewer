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
                SetLayerDisplay((int)window.Context.ActiveLayer);
            }
        }

        public StatusBarControl(MainWindow window)
        {
            this.window = window;
            OnLayerChange();
            OnMipmapChange();
            window.Context.ChangedLayer += (sender, args) => OnLayerChange();
            window.Context.ChangedMipmap += (sender, args) => OnMipmapChange();
            // image loading => no  mipmaps to mipmap 0
            window.Context.ChangedImages += (sender, args) => OnMipmapChange();

            window.ComboBoxView.SelectionChanged += ComboBoxViewOnSelectionChanged;
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
            var p = new Point(window.MousePosition.X / window.WinFormsHost.ActualWidth, 1.0f - window.MousePosition.Y / window.WinFormsHost.ActualHeight);
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

        private void SetLayerDisplay(int layer)
        {
            switch (LayerMode)
            {
                case LayerModeType.All:
                    window.TextLayer.Text = "All Layer";
                    break;
                case LayerModeType.Single:
                    window.TextLayer.Text = "Layer " + layer;
                    break;
                case LayerModeType.None:
                    window.TextLayer.Text = "No Layer";
                    break;
            }
        }

        private void SetMipmapDisplay(int mipmap)
        {
            if (window.Context.GetNumMipmaps() == 0)
                window.TextMipmap.Text = "No Mipmap";
            else
                window.TextMipmap.Text = $"Mipmap {mipmap}: {window.Context.GetWidth(mipmap)}x{window.Context.GetHeight(mipmap)}";
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
