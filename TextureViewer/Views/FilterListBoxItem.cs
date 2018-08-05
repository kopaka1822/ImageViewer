using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using OpenTK.Graphics.OpenGL;
using TextureViewer.Models.Filter;
using TextureViewer.ViewModels;
using TextureViewer.ViewModels.Filter;

namespace TextureViewer.Views
{
    public class FilterListBoxItem : ListBoxItem
    {
        public FilterModel Filter { get; }

        public FilterListBoxItem(FiltersViewModel filters, FilterModel filter, FilterParametersViewModel viewModel)
        {
            Filter = filter;

            // load images
            var imgDelete = new Image
            {
                Source = App.GetResourceImage(App.ResourceIcon.Cancel)
            };

            var imgVisible = new Image
            {
                Source = App.GetResourceImage(App.ResourceIcon.Eye)
            };

            var imgArrow = new Image
            {
                Source = new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/list_move.png",
                    UriKind.Absolute)),
                Margin = new Thickness(0.0, 0.0, 5.0, 0.0)
            };

            var btnDelete = new Button
            {
                Height = 16,
                Width = 16,
                Content = imgDelete,
                Margin = new Thickness(0.0, 0.0, 5.0, 0.0)
            };

            var btnVisible = new Button
            {
                Height = 16,
                Width = 16,
                Content = imgVisible,
                Margin = new Thickness(0.0, 0.0, 5.0, 0.0),
            };

            btnVisible.Click += (sender, args) =>
            {
                viewModel.IsVisible = !viewModel.IsVisible;
                imgVisible.Source = App.GetResourceImage(viewModel.IsVisible ? App.ResourceIcon.Eye : App.ResourceIcon.EyeClosed);
            };

            var text = new TextBlock { Text = filter.Name };

            // grid with remove, arrow, name
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });

            Grid.SetColumn(btnDelete, 0);
            grid.Children.Add(btnDelete);
            Grid.SetColumn(btnVisible, 1);
            grid.Children.Add(btnVisible);
            Grid.SetColumn(imgArrow, 2);
            grid.Children.Add(imgArrow);
            Grid.SetColumn(text, 3);
            grid.Children.Add(text);

            // add callbacks
            Content = grid;
            ToolTip = filter.Name;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;

            btnDelete.Click += (sender, args) => filters.RemoveFilter(filter);
        }
    }
}
