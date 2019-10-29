using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;

namespace ImageViewer.Commands
{
    public class GifExportCommand : Command
    {
        private readonly ModelsEx models;
        private readonly GifExportViewModel viewModel = new GifExportViewModel();

        public GifExportCommand(ModelsEx models)
        {
            this.models = models;
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

        public override void Execute()
        {
            if (models.NumEnabled != 2)
            {
                models.Window.ShowErrorDialog("Exactly two image equations should be visible for gif exporting.");
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
                models.Window.ShowErrorDialog("This mode only works with multiplier = 1.0 at the moment...");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Graphics Interchange Format (*.gif)|*.gif",
                // TODO set export directory
                // TODO set filename
            };

            if (sfd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            var dia = new GifExportDialog(viewModel);
            if (models.Window.ShowDialog(dia) != true) return;
            
            models.Gif.CreateGif(img1, img2, new GifModel.Config
            {
                Filename = sfd.FileName,
                FramesPerSecond = viewModel.FramesPerSecond,
                SliderWidth = viewModel.SliderSize,
                NumSeconds = viewModel.TotalSeconds
            });
        }
    }
}
