using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageViewer.Commands;
using ImageViewer.Commands.Helper;
using ImageViewer.Controller;
using ImageViewer.Models;

namespace ImageViewer.ViewModels.Dialog
{
    public class ImportArrayViewModel : INotifyPropertyChanged, IDisposable, IDropTarget
    {
        private readonly ModelsEx models;

        private static string GetPrettyFilename(string fullPath, int numTextures)
        {
            if (numTextures != 6) return fullPath;

            var filename = System.IO.Path.GetFileName(fullPath);
            if (filename == null) return fullPath;
            if (!filename.StartsWith("x")) return fullPath;

            // use directory name
            var path = System.IO.Path.GetDirectoryName(fullPath);
            if (path == null) return fullPath;
            var actualDir = System.IO.Path.GetFileName(path);
            if (actualDir.Length != 0) return actualDir;
            return fullPath;
        }

        public class Import2DCommand : Command
        {
            private readonly ImportArrayViewModel viewModel;

            public Import2DCommand(ModelsEx models, ImportArrayViewModel viewModel) : base(models)
            {
                this.viewModel = viewModel;
                this.viewModel.ListItems.CollectionChanged += ListItemsOnCollectionChanged;
            }

            private void ListItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                OnCanExecuteChanged();
            }

            public override bool CanExecute()
            {
                // if nothing imported yet => anything counts as long as there is one image
                if (models.Images.NumImages == 0)
                {
                    return viewModel.ListItems.Count > 0;
                }

                // otherwise the array count must match
                return models.Images.ImageType == typeof(TextureArray2D) && (viewModel.ListItems.Count == models.Images.NumLayers);
            }

            public override void Execute()
            {
                models.Window.TopmostWindow.DialogResult = true;

                var textures = viewModel.GetTextures();

                try
                {
                    var res = models.CombineToArray(textures);

                    models.Images.AddImage(res,
                        false,
                        GetPrettyFilename(viewModel.ListItems[0].Filename, textures.Count),
                        viewModel.ListItems[0].Format);
                }
                catch (Exception e)
                {
                    models.Window.ShowErrorDialog(e);
                }
            }
        }

        public class Import3DCommand : Command
        {
            private readonly ImportArrayViewModel viewModel;

            public Import3DCommand(ModelsEx models, ImportArrayViewModel viewModel) : base(models)
            {
                this.viewModel = viewModel;
                this.viewModel.ListItems.CollectionChanged += ListItemsOnCollectionChanged;
            }

            private void ListItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                OnCanExecuteChanged();
            }

            public override bool CanExecute()
            {
                if (viewModel.ListItems.Count == 0) return false;

                // if nothing imported yet => anything counts as long as there is one image
                if (models.Images.NumImages == 0)
                {
                    return true;
                }

                // otherwise the image type
                return models.Images.ImageType == typeof(Texture3D) && (viewModel.ListItems.Count == models.Images.Size.Z);
            }

            public override void Execute()
            {
                models.Window.TopmostWindow.DialogResult = true;

                var textures = viewModel.GetTextures();

                try
                {
                    var tmpArray = models.CombineToArray(textures);
                    var res = models.ConvertTo3D(tmpArray);
                    tmpArray.Dispose();

                    models.Images.AddImage(res,
                        false,
                        GetPrettyFilename(viewModel.ListItems[0].Filename, textures.Count),
                        viewModel.ListItems[0].Format);
                }
                catch (Exception e)
                {
                    models.Window.ShowErrorDialog(e);
                }
            }
        }

        public class ImageItem
        {
            public TextureArray2D Image { get; set; }
            public string Filename { get; set; }
            public string ShortFilename { get; set; }
            public ICommand DeleteCommand { get; set; }

            public GliFormat Format { get; set; }
        }

        public ObservableCollection<ImageItem> ListItems { get; private set; } = new ObservableCollection<ImageItem>();

        public ImportArrayViewModel(ModelsEx models)
        {
            this.models = models;
            this.CancelCommand = new ActionCommand(Cancel);
            this.ImportCommand = new ActionCommand(Import);
            this.Apply2DCommand = new Import2DCommand(models, this);
            this.Apply3DCommand = new Import3DCommand(models, this);
            this.SortCommand = new ActionCommand(Sort);
        }

        private void Import()
        {
            var files = models.Import.ShowImportImageDialog();
            if (files == null) return;

            foreach (var file in files)
            {
                ImportFile(file);    
            }
        }

        private bool ImportFile(string filename)
        {
            try
            {
                using (var img = IO.LoadImage(filename))
                {
                    // test if compatible

                    if (img.LayerMipmap.Layers != 1)
                        throw new Exception($"{filename}: Only single layer images are supported");

                    if (models.Images.NumImages != 0)
                    {
                        // is it compatible with already imported images
                        if(img.Size != models.Images.GetSize(0))
                            throw new Exception($"{filename}: Image resolution mismatch. Expected {models.Images.GetWidth(0)}x{models.Images.GetHeight(0)} but got {img.Size.X}x{img.Size.Y}");

                        if(img.LayerMipmap.Mipmaps != models.Images.NumMipmaps)
                            throw new Exception($"{filename}: Inconsistent amount of mipmaps. Expected {models.Images.NumMipmaps} got {img.LayerMipmap.Mipmaps}");
                    }
                    else if(ListItems.Count > 0)
                    {
                        var first = ListItems[0].Image;
                        if(img.Size != first.Size)
                            throw new Exception($"{filename}: Image resolution mismatch. Expected {first.Size.X}x{first.Size.Y} but got {img.Size.X}x{img.Size.Y}");
                        
                        if(img.LayerMipmap.Mipmaps != first.NumMipmaps)
                            throw new Exception($"{filename}: Inconsistent amount of mipmaps. Expected {first.NumMipmaps} got {img.LayerMipmap.Mipmaps}");
                    }

                    // first image
                    if(img.Is3D)
                        throw new Exception($"{filename}: Only 2D images are allowed");

                    // import image
                    var listItem = new ImageItem
                    {
                        Filename = filename,
                        ShortFilename = System.IO.Path.GetFileNameWithoutExtension(filename),
                        Image = new TextureArray2D(img),
                        Format = img.OriginalFormat
                    };
                    listItem.DeleteCommand = new ActionCommand(() => DeleteItem(listItem));
                    ListItems.Add(listItem);
                }

                return true;
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }

            return false;
        }

        private void DeleteItem(ImageItem item)
        {
            if (item == null) return;
            ListItems.Remove(item);
            item.Image?.Dispose();
        }

        private void Cancel()
        {
            models.Window.TopmostWindow.DialogResult = false;
        }

        private void Sort()
        {
            var sorted = ListItems.OrderBy(x => x.ShortFilename);
            ListItems = new ObservableCollection<ImageItem>(sorted);
            OnPropertyChanged(nameof(ListItems));
        }

        public List<TextureArray2D> GetTextures()
        {
            var res = new List<TextureArray2D>();
            foreach (var imageItem in ListItems)
            {
                res.Add(imageItem.Image);
            }
            return res;
        }

        public ICommand ImportCommand { get; }
        public ICommand Apply2DCommand { get; }
        public ICommand Apply3DCommand { get; }

        public ICommand SortCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            foreach (var imageItem in ListItems)
            {
                imageItem.Image?.Dispose();
            }
        }

        #region DRAG DROP
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ImageItem && dropInfo.TargetItem is ImageItem)
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

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ImageItem)
            {
                // move images
                var idx1 = ListItems.IndexOf(dropInfo.Data as ImageItem);
                var idx2 = dropInfo.InsertIndex;
                if (idx1 < 0 || idx2 < 0) return;
                if (idx1 == idx2) return;
                if (idx1 < idx2) idx2--;


                ListItems.Move(idx1, idx2);
            }
            else if (dropInfo.Data is DataObject)
            {
                var obj = dropInfo.Data as System.Windows.DataObject;
                var items = obj.GetFileDropList();
                if (items == null) return;

                int desiredPosition = dropInfo.InsertIndex;

                foreach (var file in items)
                {
                    if (ImportFile(file))
                    {
                        ListItems.Move(ListItems.Count - 1, desiredPosition++);
                    }
                }
            }
        }

        #endregion
    }
}
