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

namespace ImageViewer.Commands
{
    public class CubemapToLatLongCommand : Command
    {
        private readonly ModelsEx models;

        public CubemapToLatLongCommand(ModelsEx models)
        {
            this.models = models;
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
            return models.Images.NumLayers == 6;
        }

        public override void Execute()
        {
            var vm = new ResolutionViewModel(2);
            var dia = new ResolutionDialog
            {
                DataContext = vm
            };
            if (models.Window.ShowDialog(dia) != true) return;

            throw new NotImplementedException();
        }
    }
}
