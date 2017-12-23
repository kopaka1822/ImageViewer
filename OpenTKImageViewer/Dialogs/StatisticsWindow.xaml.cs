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

namespace OpenTKImageViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        readonly float[] textureData = null;

        public StatisticsWindow(MainWindow parent)
        {
            InitializeComponent();

            if (parent.Context.GetNumImages() == 0)
                return;
            if (parent.Context.GetFirstActiveTexture() < 0)
                return;
            
            parent.EnableOpenGl();

            textureData = parent.Context.GetStatisticsImageFloatData(parent.Context.GetFirstActiveTexture(), (int)parent.Context.ActiveMipmap);

            parent.DisableOpenGl();

            if (textureData == null)
                return;

            // TODO do this on a seperate thread?
            CalcStatistics();
        }

        private void CalcStatistics()
        {
            double lumiSum = 0.0;
            double redSum = 0.0;
            double greenSum = 0.0;
            double blueSum = 0.0;

            var length = textureData.Length / 4;
            for(var i = 0; i < length; ++i)
            {
                redSum += textureData[4 * i];
                greenSum += textureData[4 * i + 1];
                blueSum += textureData[4 * i + 2];

                // 0.299, 0.587, 0.114
                lumiSum += textureData[4 * i] * 0.299 + textureData[4 * i + 1] * 0.587 + textureData[i * 4 + 2] * 0.114;
            }

            SetInformation(redSum / length, greenSum / length, blueSum / length, lumiSum / length);
        }

        private void SetInformation(double redAvg, double greenAvg, double blueAvg, double lumiAvg)
        {
            boxAvgLumi.Text = lumiAvg.ToString();
            boxAvgRed.Text = redAvg.ToString();
            boxAvgGreen.Text = greenAvg.ToString();
            boxAvgBlue.Text = blueAvg.ToString();

            boxRootLumi.Text = Math.Sqrt(lumiAvg).ToString();
            boxRootRed.Text = Math.Sqrt(redAvg).ToString();
            boxRootGreen.Text = Math.Sqrt(greenAvg).ToString();
            boxRootBlue.Text = Math.Sqrt(blueAvg).ToString();
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null)
                return;
            box.SelectionStart = 0;
            box.SelectionLength = box.Text.Length;
            Clipboard.SetText(box.Text);
        }
    }
}
