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
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.Tools
{
    public class ShowPaddingCommand : Command
    {
        public ShowPaddingCommand(ModelsEx models) : base(models)
        {
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
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
            var vm = new PaddingViewModel(models);
            var dia = new PaddingDialog(vm);

            if(models.Window.ShowDialog(dia) != true) return;

            try
            {
                models.Images.PadImages(vm.LeftPad, vm.RightPad, vm.SelectedFill.Cargo, models);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
        }
    }
}
