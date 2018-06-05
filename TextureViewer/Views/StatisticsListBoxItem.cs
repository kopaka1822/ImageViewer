using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TextureViewer.Views
{
    public class StatisticsListBoxItem : ListBoxItem
    {
        public StatisticsListBoxItem()
        {
            var imgArrow = new Image
            {
                Source = new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/list_move.png",
                    UriKind.Absolute)),
                Margin = new Thickness(21.0, 0.0, 5.0, 0.0)
            };

            var text = new TextBlock { Text = "Pixel Color" };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });

            Grid.SetColumn(imgArrow, 0);
            grid.Children.Add(imgArrow);
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            Content = grid;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ToolTip = Text;

        }

        public static readonly string Text = "Pixel color for the status bar and statistics are taken from this point. Further operators are applied for display and export.";
    }
}
