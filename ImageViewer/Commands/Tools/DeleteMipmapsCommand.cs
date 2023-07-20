using System;
using System.ComponentModel;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Tools
{
    public class DeleteMipmapsCommand : Command
    {
        private readonly ModelsEx models;

        public DeleteMipmapsCommand(ModelsEx models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumMipmaps):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.NumMipmaps > 1;
        }

        public override void Execute()
        {
            try
            {
                models.Images.DeleteMipmaps();
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
        }
    }
}
