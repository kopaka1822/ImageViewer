using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands
{
    public class ShowPixelColorCommand : Command
    {
        private readonly ModelsEx models;

        public ShowPixelColorCommand(ModelsEx models)
        {
            this.models = models;
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
            return models.NumEnabled > 0 && models.Images.NumImages > 0;
        }

        public override void Execute()
        {
            var colors = new List<PixelColorDialog.Element>();
            for (int i = 0; i < models.NumPipelines; ++i)
            {
                if (models.Pipelines[i].IsEnabled)
                {
                    var tex = models.Pipelines[i].Image;
                    if(tex == null) continue;

                    var color = models.GetPixelValue(tex, new Size3(models.Display.TexelPosition.X, models.Display.TexelPosition.Y, 0),
                        models.Display.ActiveLayer, models.Display.ActiveMipmap, models.Display.TexelRadius);

                    colors.Add(new PixelColorDialog.Element(color, i, models.Statistics[i].Stats.HasAlpha));
                }
            }

            if (colors.Count > 0)
            {
                var dia = new PixelColorDialog(colors);
                models.Window.ShowDialog(dia);
            }
        }
    }
}
