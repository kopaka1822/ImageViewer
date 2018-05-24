using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TextureViewer.Models.Filter;
using TextureViewer.ViewModels;

namespace TextureViewer.Views
{
    public class FilterListBoxItem : ListBoxItem
    {
        public FilterModel Filter { get; }

        public FilterListBoxItem(FiltersViewModel filters, FilterModel filter)
        {
            Filter = filter;

            // load images
            var imgDelete = new Image
            {
                Source = new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/cancel.png", UriKind.Absolute))
            };

            var imgArrow = new Image
            {
                Source = new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/list_move.png", UriKind.Absolute))
            };
            imgArrow.Margin = new Thickness(0.0, 0.0, 5.0, 0.0);

            var btnDelete = new Button
            {
                Height = 16,
                Width = 16,
                Content = imgDelete
            };

            var text = new TextBlock { Text = filter.Name };

            // grid with name, remove, up/down
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Auto) });

            Grid.SetColumn(imgArrow, 0);
            grid.Children.Add(imgArrow);
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);
            Grid.SetColumn(btnDelete, 2);
            grid.Children.Add(btnDelete);

            // add callbacks
            Content = grid;
            ToolTip = filter.Name;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;

            btnDelete.Click += (sender, args) => filters.RemoveFilter(filter);
        }
    }
}
