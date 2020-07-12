using System;
using System.ComponentModel;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Import
{
    public class ImportEquationImageCommand : Command<int>
    {
        private readonly ModelsEx models;

        public ImportEquationImageCommand(ModelsEx models)
        {
            this.models = models;
            foreach (var pipe in models.Pipelines)
            {
                pipe.PropertyChanged += PipeOnPropertyChanged;
            }
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.Image):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute(int parameter)
        {
            return models.Pipelines[parameter].Image != null;
        }

        public override void Execute(int parameter)
        {
            // create copy of the final image
            var tex = models.Pipelines[parameter].Image;
            if (tex == null) return;

            tex = tex.Clone();

            var proposedFilename = models.Images.Images[models.Pipelines[parameter].GetFirstImageId()].Filename;
            models.Images.AddImage(tex, false, proposedFilename, GliFormat.RGBA32_SFLOAT, $"Eq{parameter + 1} {DateTime.Now:HH:mm}");
        }
    }
}
