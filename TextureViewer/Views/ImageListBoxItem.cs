using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TextureViewer.Models;

namespace TextureViewer.Views
{
    public class ImageListBoxItem : ListBoxItem
    {
        public int Id { get; }

        public ImageListBoxItem(string filename, int id, ImagesModel imagesModel)
        {
            Id = id;
            // load images
            var imgDelete = new Image
            {
                Source = new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/cancel.png", UriKind.Absolute))
            };

            var btnDelete = new Button
            {
                Height = 16,
                Width = 16,
                Content = imgDelete
            };

            var text = new TextBlock
            {
                Text = $"I{Id} - {System.IO.Path.GetFileNameWithoutExtension(filename)}",
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Auto) });

            Grid.SetColumn(text, 0);
            grid.Children.Add(text);
            Grid.SetColumn(btnDelete, 1);
            grid.Children.Add(btnDelete);

            btnDelete.Click += (sender, args) => imagesModel.DeleteImage(Id);

            Content = grid;
            ToolTip = filename;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }
    }
}
