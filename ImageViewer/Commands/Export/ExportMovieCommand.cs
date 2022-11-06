using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;
using SharpDX.D3DCompiler;

namespace ImageViewer.Commands.Export
{
    public class ExportMovieCommand : Command
    {
        private readonly ModelsEx models;
        private readonly PathManager path;

        public ExportMovieCommand(ModelsEx models)
        {
            this.models = models;
            path = models.ExportPath;
            this.models.PropertyChanged += ModelsOnPropertyChanged;
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

        private void ModelsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImageFramework.Model.Models.NumEnabled):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.NumImages > 0 && models.NumEnabled > 0;
        }

        public override async void Execute()
        {
            // make sure only one image is visible
            if (models.NumEnabled != 1)
            {
                models.Window.ShowErrorDialog("Exactly one image equation should be visible when exporting.");
                return;
            }

            // make sure that we actually have frames
            if (models.Images.NumLayers <= 1)
            {
                models.Window.ShowErrorDialog("Video export is only supported for images with multiple layers (TextureArray). Import a TextureArray or use the 'Import as Array' or 'Import Video as Texture Array' function");
                return;
            }

            if (!FFMpeg.IsAvailable())
            {
                models.Window.ShowFFMpegDialog();
                return;
            }

            // get active final image
            var id = models.GetFirstEnabledPipeline();
            var tex = models.Pipelines[id].Image;
            if (tex == null) return; // not computed?

            float multiplier = ExportCommand.GetImageMultiplier(models);

            path.InitFromEquations(models);

            var firstImageId = models.Pipelines[id].Color.FirstImageId;

            var sfd = new SaveFileDialog
            {
                Filter = "mp4 (*.mp4)|*.mp4",
                InitialDirectory = path.Directory,
                FileName = path.Filename
            };

            if (sfd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            path.UpdateFromFilename(sfd.FileName);

            var viewModel = new ExportMovieViewModel(models, sfd.FileName);
            var dia = new ExportMovieDialog(viewModel);

            if (models.Window.ShowDialog(dia) != true) return;

            var config = viewModel.GetConfig();
            config.Multiplier = multiplier;
            config.Source = tex as TextureArray2D;

            await FFMpeg.ExportMovieAsync(config, models);

            if (!models.Progress.LastTaskCancelledByUser && !String.IsNullOrEmpty(models.Progress.LastError))
            {
                models.Window.ShowErrorDialog(models.Progress.LastError);
            }
        }
    }
}
