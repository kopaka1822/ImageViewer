using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
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
    public class ImagesViewModel : INotifyPropertyChanged, IDropTarget
    {
        private readonly Models.Models models;
        private readonly WindowViewModel windowViewModel;

        public ImagesViewModel(Models.Models models, WindowViewModel windowViewModel)
        {
            this.models = models;
            this.windowViewModel = windowViewModel;
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
                case nameof(ImagesModel.ImageOrder):
                    RefreshImageList();
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

        #region DRAG DROP
        public void DragOver(IDropInfo dropInfo)
        {
            
            // enable if both items are image list box items
            if(dropInfo.Data is ImageListBoxItem && dropInfo.TargetItem is ImageListBoxItem)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }

            if(dropInfo.Data is System.Windows.DataObject)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if(dropInfo.Data is ImageListBoxItem)
            {
                // move images 
                var idx1 = ImageListItems.IndexOf(dropInfo.Data as ImageListBoxItem);
                var idx2 = dropInfo.InsertIndex;
                if (idx1 < 0 || idx2 < 0) return;
                // did the order change?
                if (idx1 == idx2) return;

                // move image want the final position of the moved image
                if (idx1 < idx2) idx2--;

                // put item from idx1 into the position it was dragged to
                models.Images.MoveImage(idx1, idx2);
            }
            else if(dropInfo.Data is DataObject)
            {
                var obj = dropInfo.Data as System.Windows.DataObject;
                var items = obj.GetFileDropList();
                if (items == null) return;

                int desiredPosition = dropInfo.InsertIndex;

                foreach(var file in items)
                {
                    if(windowViewModel.ImportImage(file))
                    {
                        // put inserted image into correct position
                        models.Images.MoveImage(models.Images.NumImages - 1, desiredPosition++);
                    }
                }
            }
        }
        #endregion
    }
}
