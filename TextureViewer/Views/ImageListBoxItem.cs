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
                Source = App.GetResourceImage(App.ResourceIcon.Cancel)
            };

            var btnDelete = new Button
            {
                Height = 16,
                Width = 16,
                Content = imgDelete,
                Margin = new Thickness(0.0, 0.0, 5.0, 0.0)
            };

            var text = new TextBlock
            {
                Text = $"I{Id} - {System.IO.Path.GetFileNameWithoutExtension(filename)}",
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.0, GridUnitType.Star) });

            Grid.SetColumn(btnDelete, 0);
            grid.Children.Add(btnDelete);
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            btnDelete.Click += (sender, args) => imagesModel.DeleteImage(Id);

            Content = grid;
            ToolTip = filename;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }
    }
}
