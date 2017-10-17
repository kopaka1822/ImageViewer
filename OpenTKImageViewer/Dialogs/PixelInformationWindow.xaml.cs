using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for PixelInformationWindow.xaml
    /// </summary>
    public partial class PixelInformationWindow : Window
    {
        public PixelInformationWindow(float r, float g, float b, float a)
        {
            InitializeComponent();
            // set values
            BoxRedBit.Text = ToBit(r);
            BoxGreenBit.Text = ToBit(g);
            BoxBlueBit.Text = ToBit(b);
            BoxAlphaBit.Text = ToBit(a);

            BoxRedFloat.Text = ToFloat(r);
            BoxGreenFloat.Text = ToFloat(g);
            BoxBlueFloat.Text = ToFloat(b);
            BoxAlphaFloat.Text = ToFloat(a);

            BoxBit.Text = 
                BoxRedBit.Text + ", " + 
                BoxGreenBit.Text + ", " + 
                BoxBlueBit.Text + ", " + 
                BoxAlphaBit.Text;

            BoxFloat.Text =
                BoxRedFloat.Text + ", " +
                BoxGreenFloat.Text + ", " +
                BoxBlueFloat.Text + ", " +
                BoxAlphaFloat.Text;

            BoxHex.Text = 
                ToHex(r) +
                ToHex(g) +
                ToHex(b) +
                ToHex(a);
        }

        private static string ToHex(float c)
        {
            var b = (byte) (c * 255);
            return b.ToString("X2");
        }

        private static string ToFloat(float c)
        {
            return ((decimal)c).ToString(new CultureInfo("en-US"));
        }

        private static string ToBit(float c)
        {
            return ((int) (c * 255)).ToString(new CultureInfo("en-US"));
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var box = sender as TextBox;
            if(box == null)
                return;
            box.SelectionStart = 0;
            box.SelectionLength = box.Text.Length;
            Clipboard.SetText(box.Text);
        }
    }
}
