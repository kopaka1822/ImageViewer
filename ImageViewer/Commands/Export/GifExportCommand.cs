using System;
using System.ComponentModel;
using System.IO;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;

namespace ImageViewer.Commands.Export
{
    public class GifExportCommand : Command
    {
        private readonly ModelsEx models;
        private readonly GifExportViewModel viewModel = new GifExportViewModel();
        private readonly PathManager path; // == models.ExportConfig.Path
        private bool askForVideo = true;

        public GifExportCommand(ModelsEx models)
        {
            this.models = models;
            this.path = models.ExportConfig.Path;
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

        public override async void Execute()
        {
            if (models.NumEnabled != 2)
            {
                models.Window.ShowErrorDialog("Exactly two image equations should be visible for exporting");
                return;
            }

            if (models.Images.ImageType != typeof(TextureArray2D))
            {
                models.Window.ShowErrorDialog("Only 2D textures are supported");
                return;
            }

            if (!FFMpeg.IsAvailable())
            {
                models.Window.ShowFFMpegDialog();
                return;
            }

            var ids = models.GetEnabledPipelines();
            // images valid?
            var img1 = models.Pipelines[ids[0]].Image;
            var img2 = models.Pipelines[ids[1]].Image;
            if (img1 == null) return;
            if (img2 == null) return;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (models.Display.Multiplier != 1.0f)
            {
                models.Window.ShowInfoDialog("Export will ignore image multiplier");
            }

            path.InitFromEquations(models);

            var sfd = new SaveFileDialog
            {
                Filter = "MPEG-4 (*.mp4)|*.mp4",
                InitialDirectory = path.Directory,
                FileName = path.Filename
            };

            if (sfd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            path.UpdateFromFilename(sfd.FileName, updateExtension: false);

            viewModel.InitTitles(models);
            var dia = new GifExportDialog(viewModel);
            if (models.Window.ShowDialog(dia) != true) return;
            
            // get tmp directory
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            System.IO.Directory.CreateDirectory(tmpDir);

            GifModel.Config config = new GifModel.Config
            {
                Filename = sfd.FileName,
                TmpDirectory = tmpDir,
                FramesPerSecond = viewModel.FramesPerSecond,
                SliderWidth = viewModel.SliderSize,
                NumSeconds = viewModel.TotalSeconds,
                Label1 = viewModel.Title1,
                Label2 = viewModel.Title2,
                Left = (TextureArray2D)img1,
                Right = (TextureArray2D)img2,
                Overlay = (TextureArray2D)models.Overlay.Overlay,
                RepeatRange = models.ZoomBox.GetXRepeatRange()
            };

            models.Gif.CreateGif(config, models.SharedModel);

            await models.Progress.WaitForTaskAsync();

            // delete tmp directory
            try
            {
                System.IO.Directory.Delete(tmpDir, true);
            }
            catch (Exception)
            {
                // ignored
            }

            if (models.Progress.LastTaskCancelledByUser) return;

            if (!String.IsNullOrEmpty(models.Progress.LastError))
            {
                models.Window.ShowErrorDialog(models.Progress.LastError);
            }
            else if(askForVideo)
            {
                askForVideo = models.Window.ShowYesNoDialog("Open video?", "Finished exporting");
                if(askForVideo)
                {
                    System.Diagnostics.Process.Start(config.Filename);
                }
            }
        }
    }
}
