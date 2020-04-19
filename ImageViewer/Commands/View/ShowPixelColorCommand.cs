using System.Collections.Generic;
using System.ComponentModel;
using ImageFramework.Model;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.View
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
            if (!models.Display.PrevTexelPosition.HasValue)
            {
                models.Window.ShowInfoDialog("No pixel selected");
                return;
            }

            var tp = models.Display.PrevTexelPosition.Value;

            var colors = new List<PixelColorDialog.Element>();
            for (int i = 0; i < models.NumPipelines; ++i)
            {
                if (models.Pipelines[i].IsEnabled)
                {
                    var tex = models.Pipelines[i].Image;
                    if(tex == null) continue;

                    var color = models.GetPixelValue(tex, new Size3(tp.X, tp.Y, tp.Z),
                        models.Display.ActiveLayerMipmap, models.Display.TexelRadius);

                    colors.Add(new PixelColorDialog.Element(color, i, models.Statistics[i].Stats.HasAlpha));
                }
            }

            if (colors.Count > 0)
            {
                var dia = new PixelColorDialog(colors, tp, models.Images.Is3D);
                models.Window.ShowDialog(dia);
            }
        }
    }
}
