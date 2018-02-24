using System;
using System.Collections.Generic;
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
using OpenTK.Graphics.OpenGL4;
using GlFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace OpenTKImageViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        public enum FileFormat
        {
            Png,
            Bmp,
            Hdr,
            Pfm
        }

        public class FormatComboBox : ComboBoxItem
        {
            public GlFormat Format { get; set; }
        }

        private MainWindow parent;

        // properties
        public int SelectedLayer => BoxLayer.SelectedIndex;

        public int SelectedMipmap => BoxMipmaps.SelectedIndex;

        public GlFormat SelectedFormat
        {
            get
            {
                var box = (FormatComboBox) BoxFormat.Items[BoxFormat.SelectedIndex];
                return box.Format;
            }
        }

        public OpenTK.Graphics.OpenGL4.PixelType SelectedPixelType { get; set; }

        public ExportWindow(MainWindow parent, string filename, 
            FileFormat format, GlFormat defaultFormat)
        {
            this.parent = parent;
            InitializeComponent();
            BoxFilename.Text = filename;
            SelectedPixelType = GetPixelType(format);

            GenerateLayerItems();
            GenerateMipmapItems();
            GenerateFormatItems(format, defaultFormat);
        }

        void UpdateBoxDesing(ComboBox box)
        {
            if (box.Items.Count < 2)
            {
                box.IsEnabled = false;
            }
        }

        private void GenerateLayerItems()
        {
            BoxLayer.Items.Clear();
            for (int i = 0; i < parent.Context.GetNumLayers(); ++i)
            {
                BoxLayer.Items.Add(new ComboBoxItem { Content = "Layer " + i });
            }
            BoxLayer.SelectedIndex = (int)parent.Context.ActiveLayer;

            UpdateBoxDesing(BoxLayer);
        }

        private void GenerateMipmapItems()
        {
            BoxMipmaps.Items.Clear();
            for (int i = 0; i < parent.Context.GetNumMipmaps(); ++i)
            {
                BoxMipmaps.Items.Add(new ComboBoxItem
                {
                    Content = "Mipmap " + i + " - "
                              + parent.Context.GetWidth(i) + "x" + parent.Context.GetHeight(i)
                });
            }
            BoxMipmaps.SelectedIndex = (int)parent.Context.ActiveMipmap;
            UpdateBoxDesing(BoxMipmaps);
        }

        private void GenerateFormatItems(FileFormat format, GlFormat defaultFormat)
        {
            int defaultIndex = -1;
            BoxFormat.Items.Clear();

            int idx = 0;
            foreach (var supportedFormat in GetSupportedFormats(format))
            {
                if (supportedFormat == defaultFormat)
                    defaultIndex = idx;

                BoxFormat.Items.Add(new FormatComboBox
                {
                    Content = supportedFormat.ToString().ToUpper(),
                    Format = supportedFormat
                });

                ++idx;
            }
            // select last item
            if (defaultIndex < 0)
                BoxFormat.SelectedIndex = BoxFormat.Items.Count - 1;
            else
                BoxFormat.SelectedIndex = defaultIndex;
            UpdateBoxDesing(BoxFormat);
        }

        private static List<GlFormat> GetSupportedFormats(FileFormat format)
        {
            List<GlFormat> res = new List<GlFormat>();
            switch (format)
            {
                case FileFormat.Png:
                    res.Add(GlFormat.Red);
                    res.Add(GlFormat.Green);
                    res.Add(GlFormat.Blue);
                    res.Add(GlFormat.Rg);
                    res.Add(GlFormat.Rgb);
                    res.Add(GlFormat.Rgba);
                    break;
                case FileFormat.Bmp:
                    res.Add(GlFormat.Red);
                    res.Add(GlFormat.Green);
                    res.Add(GlFormat.Blue);
                    res.Add(GlFormat.Rg);
                    res.Add(GlFormat.Rgb);
                    break;
                case FileFormat.Hdr:
                case FileFormat.Pfm:
                    res.Add(GlFormat.Red);
                    res.Add(GlFormat.Green);
                    res.Add(GlFormat.Blue);
                    res.Add(GlFormat.Rgb);
                    break;
            }
            return res;
        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        OpenTK.Graphics.OpenGL4.PixelType GetPixelType(FileFormat format)
        {
            switch (format)
            {
                case FileFormat.Bmp:
                case FileFormat.Png:
                    return PixelType.UnsignedByte;
                case FileFormat.Hdr:
                case FileFormat.Pfm:
                    return PixelType.Float;
            }
            return PixelType.Byte;
        }
    }
}
