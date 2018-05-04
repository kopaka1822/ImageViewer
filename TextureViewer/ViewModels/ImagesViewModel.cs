using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class ImagesViewModel : INotifyPropertyChanged
    {
        public class ImageListItem : ListBoxItem
        {
            public int Id { get; }

            public ImageListItem(string filename, int id, ImagesModel imagesModel)
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

                var grid = new Grid {  };
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

        private readonly Models.Models models;

        public ImagesViewModel(Models.Models models)
        {
            this.models = models;
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object o, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    RefreshImageList();
                    break;
            }
        }

        public ObservableCollection<ImageListItem> ImageListItems { get; } = new ObservableCollection<ImageListItem>();
        public ImageListItem SelectedImageListItem { get; set; }

        private void RefreshImageList()
        {
            SelectedImageListItem = null;

            ImageListItems.Clear();
            for (var i = 0; i < models.Images.NumImages; ++i)
            {
                var item = new ImageListItem(models.Images.GetFilename(i), i, models.Images);
                ImageListItems.Add(item);
            }

            OnPropertyChanged(nameof(ImageListItems));
            OnPropertyChanged(nameof(SelectedImageListItem));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
