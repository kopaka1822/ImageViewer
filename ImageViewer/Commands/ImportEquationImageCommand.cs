using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageViewer.Models;

namespace ImageViewer.Commands
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

            models.Images.AddImage(tex, $"Eq {parameter + 1} " + DateTime.Now.ToString("HH-mm"), GliFormat.RGBA32_SFLOAT_PACK32);
        }
    }
}
