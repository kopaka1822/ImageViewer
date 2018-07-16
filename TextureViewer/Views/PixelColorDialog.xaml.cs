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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TextureViewer.Utility;
using TextureViewer.ViewModels.Dialog;

namespace TextureViewer.Views
{
    /// <summary>
    /// Interaction logic for PixelColorDialog.xaml
    /// </summary>
    public partial class PixelColorDialog : Window
    {
        public struct Element
        {
            public Element(Color color, int id)
            {
                this.color = color;
                this.imageId = id;
            }

            public Color color;
            public int imageId;
        }

        public PixelColorDialog(List<Element> colors)
        {
            InitializeComponent();

            foreach(var color in colors)
            {
                ColorStackPanel.Children.Add(CreateGrid(color));
            }
        }

        private UIElement CreateGrid(Element element)
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star)});
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            g.Children.Add(GetTextBlock("Linear:", 0, 0));
            g.Children.Add(GetTextBlock("Srgb:", 1, 0));
            g.Children.Add(GetTextBlock("Hex:", 2, 0));

            g.Children.Add(GetTextBox("Linear", 0, 1));
            g.Children.Add(GetTextBox("Srgb", 1, 1));
            g.Children.Add(GetTextBox("Hex", 2, 1));

            g.DataContext = new PixelColorViewModel(element.color);

            var groupBox = new GroupBox();
            groupBox.Header = "Equation " + (element.imageId + 1).ToString();
            groupBox.Margin = new Thickness(0.0, 0.0, 0.0, 5.0);

            groupBox.Content = g;

            return groupBox;
        }

        private TextBlock GetTextBlock(string title, int row, int column)
        {
            var tb = new TextBlock();
            tb.Text = title;
            tb.Margin = new Thickness(0.0, 0.0, 2.0, 2.0);
            Grid.SetColumn(tb, column);
            Grid.SetRow(tb, row);
            return tb;
        }

        private StatisticTextBox GetTextBox(string bindingPath, int row, int column)
        {
            var tb = new StatisticTextBox();
            tb.Margin = new Thickness(0.0, 0.0, 0.0, 2.0);

            var binding = new Binding();
            binding.Path = new PropertyPath(bindingPath);
            binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(tb, TextBox.TextProperty, binding);

            Grid.SetColumn(tb, column);
            Grid.SetRow(tb, row);
            return tb;
        }

        private void OkOnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
