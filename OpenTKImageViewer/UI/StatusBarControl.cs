using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OpenTK;
using OpenTKImageViewer.View;

namespace OpenTKImageViewer.UI
{
    public class StatusBarControl
    {
        public enum LayerModeType
        {
            All,
            Single,
            SingleDeactivated,
            None
        }

        public enum PixelDisplayType
        {
            Bit,
            Float
        }

        class ModeBoxItem : ComboBoxItem
        {
            public ImageViewType ViewType { get; set; }
        }

        private readonly MainWindow window;
        private int pixelRadius = 0;
        private int lastMouseX = 0;
        private int lastMouseY = 0;

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
        public PixelDisplayType PixelDisplay { get; set; } = PixelDisplayType.Bit;
        public int PixelRadius
        {
            get => pixelRadius;
            set
            {
                if (value >= 1)
                    pixelRadius = value;
            }
        }

        public bool PixelShowAlpha { get; set; } = false;

        public StatusBarControl(MainWindow window)
        {
            this.window = window;
            UpdateLayerBox();
            OnLayerChange();
            OnMipmapChange();
            UpdateMipmapBox();
            OnImageChange();

            window.Context.ChangedLayer += (sender, args) => OnLayerChange();
            window.Context.ChangedMipmap += (sender, args) => OnMipmapChange();
            // image loading => no mipmaps to mipmap 0
            window.Context.ChangedImages += (sender, args) => { OnMipmapChange(); UpdateMipmapBox(); OnImageChange(); };

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
            lastMouseX = x;
            lastMouseY = y;
            if (window.Context.GetCpuTexture() != null)
            {
                window.TextMousePositionColor.Text = GetColorString(GetPixelColor(x,y));
            }
        }

        /// <summary>
        /// retrieves the pixel color for the last captured mouse position
        /// </summary>
        /// <returns></returns>
        public Vector4 GetCurrentPixelColor()
        {
            if(window.Context.GetCpuTexture() == null)
                return new Vector4(0.0f);
            return GetPixelColor(lastMouseX, lastMouseY);
        }

        private Vector4 GetPixelColor(int x, int y)
        {
            var cpuTex = window.Context.GetCpuTexture();
            var sum = new Vector4(0.0f);
            for(var curX = x - PixelRadius; curX < x + PixelRadius + 1; ++curX)
            for (var curY = y - PixelRadius; curY < y + PixelRadius + 1; ++curY)
                sum += cpuTex.GetPixel(curX, curY, (int) window.Context.ActiveLayer,
                    (int) window.Context.ActiveMipmap);

            var w = 1 + 2 * PixelRadius;
            return sum / (w * w);
        }

        private string GetColorString(Vector4 v)
        {
            if(PixelDisplay == PixelDisplayType.Bit)
                return $"{(int)(v.X * 255.0f)} {(int)(v.Y * 255.0f)} {(int)(v.Z * 255.0f)}" + (PixelShowAlpha?$" {(int)(v.W * 255.0f)}":"");

            return $"{v.X:0.00} {v.Y:0.00} {v.Z:0.00}" + (PixelShowAlpha?$" {v.W:0.00}":"");
        }

        /// <summary>
        /// mouse coordinates in range [-1.0, 1.0]
        /// </summary>
        /// <returns></returns>
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
                case LayerModeType.SingleDeactivated:
                case LayerModeType.Single:
                    var selectedLayer = window.Context.ActiveLayer;
                    for (int i = 0; i < window.Context.GetNumLayers(); ++i)
                        window.ComboBoxLayer.Items.Add(new ComboBoxItem {Content = "Layer " + i});
                    window.Context.ActiveLayer = selectedLayer;
                    window.ComboBoxLayer.SelectedIndex = (int) selectedLayer;
                    break;
                case LayerModeType.None:
                    window.ComboBoxLayer.Items.Add(new ComboBoxItem {Content = "No Layer"});
                    break;
            }

            window.ComboBoxLayer.IsEnabled = true;
            if (window.ComboBoxLayer.Items.Count == 1)
                window.ComboBoxLayer.SelectedIndex = 0;
            window.ComboBoxLayer.IsEnabled = window.ComboBoxLayer.Items.Count >= 2 && LayerMode != LayerModeType.SingleDeactivated;
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

        private void OnImageChange()
        {
            PixelShowAlpha = window.Context.HasAlpha();
            // change to decimal display if hdr image was added
            if (window.Context.HasHdr())
                PixelDisplay = StatusBarControl.PixelDisplayType.Float;
        }
    }
}
