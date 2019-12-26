using System.ComponentModel;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Tools
{
    public class GenerateMipmapsCommand : Command
    {
        private readonly ModelsEx models;

        public GenerateMipmapsCommand(ModelsEx models)
        {
            this.models = models;
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
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
            return models.Images.NumMipmaps == 1;
        }

        public override void Execute()
        {
            models.Images.GenerateMipmaps();
        }
    }
}
