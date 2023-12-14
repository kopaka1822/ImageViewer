using System;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageViewer.Controller;

namespace ImageViewer.Commands.Import
{
    public class ReloadImagesCommand : Command
    {
        public ReloadImagesCommand(ModelsEx models) : base(models)
        {
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.NumImages > 0;
        }

        public override async Task ExecuteAsync()
        {
            for (var index = 0; index < models.Images.Images.Count; index++)
            {
                var img = models.Images.Images[index];
                if (img.LastModified == null) continue;
                if (!File.Exists(img.Filename)) continue;

                var lastModify = File.GetLastWriteTime(img.Filename);

                // was the file modified?
                if (lastModify == img.LastModified) continue;

                // reload file
                IO.TexInfo tex = null;
                try
                {
                    tex = await IO.LoadImageTextureAsync(img.Filename, models.Progress);
                    models.Images.ReplaceImage(index, tex.Texture, tex.OriginalFormat);
                    tex = null; // ownership belongs to ImagesModel
                }
                catch (Exception e)
                {
                    if (models.Progress.LastTaskCancelledByUser) return; // user does not want to reload anymore

                    models.Window.ShowErrorDialog(e);
                }
                finally
                {
                    tex?.Texture.Dispose();
                }
            }
        }
    }
}
