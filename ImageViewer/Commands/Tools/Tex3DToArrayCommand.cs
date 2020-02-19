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
using ImageViewer.Models.Display;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.Tools
{
    public class Tex3DToArrayCommand : Command
    {
        private readonly ModelsEx models;

        public Tex3DToArrayCommand(ModelsEx models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
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
            return models.Images.ImageType == typeof(Texture3D);
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

            var vm = new Tex3DToArrayViewModel(srcTex.Size);
            if (models.Display.ExtendedViewData is Single3DDisplayModel displayEx)
            {
                vm.SelectedAxis = vm.AxisList.Find(i => i.Cargo == displayEx.FixedAxis);
                vm.FirstSlice = displayEx.FixedAxisSlice;
                vm.LastSlice = displayEx.FixedAxisSlice;
            }
            
            var dia = new Tex3DToArrayDialog(vm);

            if (models.Window.ShowDialog(dia) != true) return;

            var tex = models.ConvertTo2DArray((Texture3D) srcTex, vm.FreeAxis1, vm.FreeAxis2, vm.FirstSlice,
                vm.LastSlice - vm.FirstSlice + 1);

            // clear all textures
            models.Reset();
            models.Images.AddImage(tex, texName, origFormat);
        }
    }
}
