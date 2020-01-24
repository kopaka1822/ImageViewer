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
using ImageFramework.Utility;

namespace ImageViewer.Views.Dialog
{
    /// <summary>
    /// Interaction logic for ColorPickerDialog.xaml
    /// </summary>
    public partial class ColorPickerDialog : Window
    {
        private readonly ImageFramework.Utility.Color intialColor;

        public ColorPickerDialog(ImageFramework.Utility.Color color)
        {
            this.intialColor = color;
            InitializeComponent();

            ColorPicker.SelectedColor = System.Windows.Media.Color.FromScRgb(1.0f, color.Red, color.Green, color.Blue);
        }

        public ImageFramework.Utility.Color Color
        {
            get
            {
                if (!ColorPicker.SelectedColor.HasValue) return intialColor;

                var c = ColorPicker.SelectedColor.Value;
                return new ImageFramework.Utility.Color(c.ScR, c.ScG, c.ScB, 1.0f);
            }
        }

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
