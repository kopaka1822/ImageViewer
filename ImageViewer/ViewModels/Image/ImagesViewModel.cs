using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageViewer.Controller;
using ImageViewer.Models;
using ImageViewer.Views.List;

namespace ImageViewer.ViewModels.Image
{
    public class ImagesViewModel : INotifyPropertyChanged, IDropTarget
    {
        private readonly ModelsEx models;
        private readonly ImportDialogController import;
        private readonly string versionString;
        public ImagesViewModel(ModelsEx models)
        {
            this.models = models;
            import = new ImportDialogController(models);
            models.Images.PropertyChanged += ImagesOnPropertyChanged;

            versionString = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        }

        private void ImagesOnPropertyChanged(object o, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    RefreshImageList();
                    OnPropertyChanged(nameof(WindowTitle));
                    break;
                case nameof(ImagesModel.ImageAlias):
                    OnPropertyChanged(nameof(WindowTitle));
                    break;
                case nameof(ImagesModel.ImageOrder):
                    RefreshImageList();
                    OnPropertyChanged(nameof(WindowTitle));
                    break;
                case nameof(ImagesModel.ImageType):
                    OnPropertyChanged(nameof(Is2D));
                    OnPropertyChanged(nameof(Is3D));
                    OnPropertyChanged(nameof(IsEmptyOr2D));
                    OnPropertyChanged(nameof(IsEmptyOr3D));
                    break;
            }
        }

        public ObservableCollection<ImageItemViewModel> ImageListItems { get; } = new ObservableCollection<ImageItemViewModel>();

        public string WindowTitle
        {
            get
            {
                if (this.models.Images.NumImages == 0) return "Texture Viewer " + versionString;
                var res = models.Images.Images[0].Alias;

                if (this.models.Images.NumImages > 1)
                {
                    res += $" ({models.Images.NumImages})";
                }

                return res;
            }
        }

        public bool Is3D => models.Images.ImageType == typeof(Texture3D);
        public bool Is2D => models.Images.ImageType == typeof(TextureArray2D);
        public bool IsEmptyOr3D => models.Images.ImageType != typeof(TextureArray2D);
        public bool IsEmptyOr2D => models.Images.ImageType != typeof(Texture3D);

        private void RefreshImageList()
        {
            ImageListItems.Clear();
            for (var i = 0; i < models.Images.NumImages; ++i)
            {
                var item = new ImageItemViewModel(models, i);
                ImageListItems.Add(item);
            }

            OnPropertyChanged(nameof(ImageListItems));
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
            if (dropInfo.Data is ImageItemViewModel && dropInfo.TargetItem is ImageItemViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }

            if (dropInfo.Data is System.Windows.DataObject)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ImageItemViewModel vm)
            {
                // move images 
                var idx1 = ImageListItems.IndexOf(vm);
                var idx2 = dropInfo.InsertIndex;
                if (idx1 < 0 || idx2 < 0) return;
                // did the order change?
                if (idx1 == idx2) return;

                // move image want the final position of the moved image
                if (idx1 < idx2) idx2--;

                // put item from idx1 into the position it was dragged to
                models.Images.MoveImage(idx1, idx2);
            }
            else if (dropInfo.Data is DataObject obj)
            {
                var items = obj.GetFileDropList();

                int desiredPosition = dropInfo.InsertIndex;

                foreach (var file in items)
                {
                    await import.ImportImageAsync(file);
                    
                    // put inserted image into correct position
                    models.Images.MoveImage(models.Images.NumImages - 1, desiredPosition++);
                }
            }
        }
        #endregion
    }
}
