using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Tools
{
    public class ArrayTo3DCommand : Command
    {
        public ArrayTo3DCommand(ModelsEx models) : base(models)
        {
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumLayers):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.NumLayers > 1;
        }

        public override void Execute()
        {
            // make sure only one image is visible
            if (models.NumEnabled != 1)
            {
                models.Window.ShowErrorDialog("Exactly one image equation should be visible for conversion.");
                return;
            }

            var pipeId = models.GetFirstEnabledPipeline();
            var srcTex = models.Pipelines[pipeId].Image;
            if (srcTex == null) return; // not yet computed?

            var firstImage = models.Images.Images[models.Pipelines[pipeId].Color.FirstImageId];
            var texName = firstImage.Filename;
            var origFormat = firstImage.OriginalFormat;
            var texAlias = firstImage.Alias;

            Texture3D tex = null;
            try
            {
                tex = models.ConvertTo3D((TextureArray2D)srcTex);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
                return;
            }

            // clear all images
            models.Reset();
            models.Images.AddImage(tex, false, texName, origFormat, texAlias);
        }
    }
}
