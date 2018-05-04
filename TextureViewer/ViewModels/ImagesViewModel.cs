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
                    Text = $"I{Id} - {RemoveFilePath(filename)}",
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

            private static string RemoveFilePath(string file)
            {
                var idx = file.LastIndexOf("\\", StringComparison.Ordinal);
                if (idx > 0)
                {
                    return file.Substring(idx + 1);
                }
                return file;
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

        /// <summary>
        /// opens a file dialoge to import images
        /// </summary>
        public void ImportImage()
        {
            // TODO add multi select
            var ofd = new Microsoft.Win32.OpenFileDialog {Multiselect = false};
            // TODO set initial directory

            if (ofd.ShowDialog() != true) return;

            // TODO set new inital directory
            // load image
            try
            {
                var imgs = ImageLoader.LoadImage(ofd.FileName);
                models.Images.AddImages(imgs);
            }
            catch (Exception e)
            {
                // TODO put window reference here
                App.ShowErrorDialog(null, e.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
