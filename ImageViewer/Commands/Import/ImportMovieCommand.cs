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

namespace ImageViewer.Commands.Import
{
    public class ImportMovieCommand : Command
    {
        private readonly ModelsEx models;

        public ImportMovieCommand(ModelsEx models)
        {
            this.models = models;
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
            var file = Controller.ImportDialogController.ShowSingleFileImportImageDialog(models);
            if (file == null) return; // nothing picked

            // get metadata
            var meta = FFMpeg.GetMovieMetadata(file);

            // inspect metadata, if it is not compatible show error message
            if (meta.FrameCount > Device.MAX_TEXTURE_2D_ARRAY_DIMENSION)
            {
                models.Window.ShowErrorDialog($"Video frame count {meta.FrameCount} is higher than the supported number of layers {Device.MAX_TEXTURE_2D_ARRAY_DIMENSION}");
                return;
            }

            if (models.Images.NumImages != 0)
            {
                // test for compability with existing data
                if (models.Images.NumLayers != meta.FrameCount)
                {
                    models.Window.ShowErrorDialog($"Video frame count {meta.FrameCount} does not match layer count of opened image {models.Images.NumLayers}");
                    return;
                }
            }

            try
            {
                // image should be fully compatible
                var tex = await FFMpeg.ImportMovie(meta, 0, meta.FrameCount, models);

                if (models.Images.NumImages == 0)
                {
                    // use movie fps as reference
                    models.Settings.MovieFps = meta.FramesPerSecond;
                }

                models.Images.AddImage(tex, true, file, GliFormat.RGB8_SRGB);

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
