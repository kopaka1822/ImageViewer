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

namespace ImageViewer.Views.Dialog
{
    /// <summary>
    /// Interaction logic for ArrowDialog.xaml
    /// </summary>
    public partial class ArrowDialog : Window
    {
        private readonly ImageFramework.Utility.Color initialColor;
        private readonly int initialStrokeWidth;
        public ArrowDialog(ImageFramework.Utility.Color color, int strokeWidth)
        {
            this.initialColor = color;
            initialStrokeWidth = strokeWidth;
            InitializeComponent();

            ColorPicker.SelectedColor = System.Windows.Media.Color.FromScRgb(1.0f, color.Red, color.Green, color.Blue);
            BorderSizeBox.Value = strokeWidth;
        }

        public ImageFramework.Utility.Color Color
        {
            get
            {
                if (!ColorPicker.SelectedColor.HasValue) return initialColor;

                var c = ColorPicker.SelectedColor.Value;
                return new ImageFramework.Utility.Color(c.ScR, c.ScG, c.ScB, 1.0f);
            }
        }

        public int StrokeWidth => BorderSizeBox.Value ?? initialStrokeWidth;

        private void Apply_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
