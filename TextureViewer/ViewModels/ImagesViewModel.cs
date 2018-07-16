using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TextureViewer.Annotations;
using TextureViewer.Models;
using TextureViewer.Views;

namespace TextureViewer.ViewModels
{
    public class ImagesViewModel : INotifyPropertyChanged
    {
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
                    OnPropertyChanged(nameof(WindowTitle));
                    break;
            }
        }

        public ObservableCollection<ImageListBoxItem> ImageListItems { get; } = new ObservableCollection<ImageListBoxItem>();
        public ImageListBoxItem SelectedImageListItem { get; set; }

        public string WindowTitle
        {
            get
            {
                if (this.models.Images.NumImages == 0) return "Texture Viewer";
                var res = System.IO.Path.GetFileNameWithoutExtension(models.Images.GetFilename(0));

                if (this.models.Images.NumImages > 1)
                {
                    res += $" ({models.Images.NumImages})";
                }

                return res;
            }
        }

        private void RefreshImageList()
        {
            SelectedImageListItem = null;

            ImageListItems.Clear();
            for (var i = 0; i < models.Images.NumImages; ++i)
            {
                var item = new ImageListBoxItem(models.Images.GetFilename(i), i, models.Images);
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
