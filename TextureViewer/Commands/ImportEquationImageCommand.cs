using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Models;
using TextureViewer.ViewModels;

namespace TextureViewer.Commands
{
    public class ImportEquationImageCommand : Command<int>
    {
        private readonly Models.Models models;

        public ImportEquationImageCommand(Models.Models models)
        {
            this.models = models;
            for (var i = 0; i < models.FinalImages.NumImages; ++i)
            {
                models.FinalImages.Get(i).PropertyChanged += FinalImageOnPropertyChanged;
            }
        }

        private void FinalImageOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(FinalImageModel.Texture):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute(int parameter)
        {
            return models.FinalImages.Get(parameter).Texture != null;
        }

        public override void Execute(int parameter)
        {
            var img = models.FinalImages.Get(parameter).Texture;
            if(img == null) return;

            // copy
            models.GlContext.Enable();
            var copy = img.Clone();
            models.GlContext.Disable();

            models.Images.AddImage(new ImagesModel.TextureArray2DInformation
            {
                Image = copy,
                IsGrayscale = models.Images.IsGrayscale,
                Name = $"Equation {parameter + 1} " + DateTime.Now.ToString("HH_mm"),
                IsAlpha = models.Images.IsAlpha
            });
        }
    }
}
