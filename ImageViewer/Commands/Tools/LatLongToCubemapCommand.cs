using System;
using System.ComponentModel;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.Tools
{
    public class LatLongToCubemapCommand : Command
    {
        private readonly ModelsEx models;

        public LatLongToCubemapCommand(ModelsEx models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumLayers):
                case nameof(ImagesModel.ImageType):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.NumLayers == 1 && models.Images.ImageType == typeof(TextureArray2D);
        }

        public override void Execute()
        {
            // make sure only one image is visible
            if (models.NumEnabled != 1)
            {
                models.Window.ShowErrorDialog("Exactly one image equation should be visible when exporting.");
                return;
            }

            var pipeId = models.GetFirstEnabledPipeline();
            var srcTex = models.Pipelines[pipeId].Image;
            if (srcTex == null) return; // not yet computed?

            var firstImage = models.Images.Images[models.Pipelines[pipeId].Color.FirstImageId];
            var texName = firstImage.Filename;
            var origFormat = firstImage.OriginalFormat;
            var texAlias = firstImage.Alias;

            var vm = new ResolutionViewModel(1);
            var dia = new ResolutionDialog
            {
                DataContext = vm
            };
            if (models.Window.ShowDialog(dia) != true) return;

            TextureArray2D tex = null;
            try
            {
                tex = models.ConvertToCubemap((TextureArray2D)srcTex, vm.Width);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
                return;
            }
            
            // clear all images
            models.Reset();

            models.Images.AddImage(tex, false, texName, origFormat, texAlias);
        }
    }
}
