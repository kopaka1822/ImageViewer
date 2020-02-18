using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;

namespace ImageViewer.ViewModels.Image
{
    public class ImageItemViewModel : INotifyPropertyChanged
    {
        private readonly ImagesModel images;

        public ImageItemViewModel(ImagesModel images, int id)
        {
            this.images = images;
            this.Id = id;
            var imgData = images.Images[id];

            Prefix = $"I{id} - ";
            ToolTip = imgData.Filename + "\n" + imgData.OriginalFormat;

            if (imgData.Alias.StartsWith("__imported"))
            {
                imageName = "";
                images.RenameImage(id, "");
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke((Action)OnRename);
            }
            else imageName = imgData.Alias;

            RenameCommand = new ActionCommand(OnRename);
            DeleteCommand = new ActionCommand(() => images.DeleteImage(id));
        }

        public int Id { get; }

        public string Prefix { get; }

        public string ToolTip { get; }

        private string imageName;

        public string ImageName
        {
            get => imageName;
            set
            {
                if (value == null || value == imageName) return;
                imageName = value;
                OnPropertyChanged(nameof(ImageName));
                IsRenaming = false;
                images.RenameImage(Id, imageName);
            }
        }

        private bool isRenaming = false;

        public bool IsRenaming
        {
            get => isRenaming;
            set
            {
                if (value == isRenaming) return;
                isRenaming = value;
                OnPropertyChanged(nameof(IsRenaming));
            }
        }

        private void OnRename()
        {
            IsRenaming = true;
        }

        public ICommand RenameCommand { get; }

        public ICommand DeleteCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
