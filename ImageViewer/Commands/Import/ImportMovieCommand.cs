using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.Import
{
    public class ImportMovieCommand : Command
    {
        private readonly ModelsEx models;
        private ImportMovieViewModel viewModel;

        public ImportMovieCommand(ModelsEx models)
        {
            this.models = models;
            viewModel = new ImportMovieViewModel(models);
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.ImageType):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.ImageType == null || models.Images.ImageType == typeof(TextureArray2D);
        }

        public override async void Execute()
        {
            if (!FFMpeg.IsAvailable())
            {
                models.Window.ShowFFMpegDialog();
                return;
            }

            var file = ImportModel.ShowSingleFileImportImageDialog(models);
            if (file == null) return; // nothing picked

            // try to get metadata
            FFMpeg.Metadata meta;
            try
            {
                meta = await FFMpeg.GetMovieMetadataAsync(file, models);
            }
            catch (Exception e)
            {
                if (!models.Progress.LastTaskCancelledByUser)
                    models.Window.ShowErrorDialog(e);
                return;
            }

            var firstFrame = 0;
            var frameCount = meta.FrameCount;

            if (models.Images.NumImages != 0 && models.Images.NumLayers > meta.FrameCount)
            {
                // frame count of imported video is too small and cannot be adjusted
                models.Window.ShowErrorDialog($"Video frame count {meta.FrameCount} does not match layer count of opened image {models.Images.NumLayers}");
                return;
            }

            // indicated if the user needs a window to adjust the frame count
            var openFrameSelection =
                (models.Images.NumImages == 0) || // show on import of first video
                (models.Images.NumImages != 0 && models.Images.NumLayers != meta.FrameCount); // show when imported video frame count does not match

            if (openFrameSelection)
            {
                int? requiredCount = null;
                if(models.Images.NumImages != 0) requiredCount = models.Images.NumLayers;
                viewModel.Init(meta, requiredCount);
                var dia = new ImportMovieDialog(viewModel);
                if(models.Window.ShowDialog(dia) != true) return;
                // obtain results
                viewModel.GetFirstFrameAndFrameCount(out firstFrame, out frameCount);
            }

            try
            {
                // image should be fully compatible
                var tex = await FFMpeg.ImportMovieAsync(meta, firstFrame, frameCount, models);

                if (models.Images.NumImages == 0)
                {
                    // use movie fps as reference
                    models.Settings.MovieFps = meta.FramesPerSecond;
                }

                models.Images.AddImage(tex, false, file, GliFormat.RGB8_SRGB);

                if (models.Settings.MovieFps != meta.FramesPerSecond)
                    models.Window.ShowInfoDialog($"The FPS count of the imported video ({meta.FramesPerSecond}) does not match the existing FPS count ({models.Settings.MovieFps}). The previous FPS count will be kept.");
            }
            catch (Exception e)
            {
                if(!models.Progress.LastTaskCancelledByUser)
                    models.Window.ShowErrorDialog(e);
            }
        }
    }
}
