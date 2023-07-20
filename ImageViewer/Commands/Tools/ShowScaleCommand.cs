using System;
using System.ComponentModel;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.Tools
{
    public class ShowScaleCommand : Command
    {
        private readonly Models.ModelsEx models;

        public ShowScaleCommand(ModelsEx models)
        {
            this.models = models;
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                case nameof(ImagesModel.ImageType):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.NumImages > 0 && models.Images.ImageType == typeof(TextureArray2D);
        }

        public override void Execute()
        {
            var vm = new ScaleViewModel(models);
            var dia = new ScaleDialog(vm);

            if (models.Window.ShowDialog(dia) != true) return;

            try
            {
                models.Images.ScaleImages(new Size3(vm.Width, vm.Height), models.Scaling);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
        }
    }
}
