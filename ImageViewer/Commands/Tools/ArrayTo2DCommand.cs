using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using static System.Net.Mime.MediaTypeNames;

namespace ImageViewer.Commands.Tools
{
    public class ArrayTo2DCommand : Command
    {
        public ArrayTo2DCommand(ModelsEx models) : base(models)
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
            if (texName != null && Path.HasExtension(texName)) texName = texName.Substring(0, texName.LastIndexOf('.'));
            var origFormat = firstImage.OriginalFormat;
            var texAlias = firstImage.Alias;

            List<TextureArray2D> textures = null;
            try
            {
                textures = models.ConvertTo2D((TextureArray2D)srcTex);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e.Message);
                return;
            }

            // clear all images
            models.Reset();
            for (var i = 0; i < textures.Count; i++)
            {
                models.Images.AddImage(textures[i], false, texName != null ? texName + i : null, origFormat, texAlias + i);
            }
        }
    }
}
