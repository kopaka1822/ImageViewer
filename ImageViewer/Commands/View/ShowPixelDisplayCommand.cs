using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.View
{
    public class ShowPixelDisplayCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public ShowPixelDisplayCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute()
        {
            var dia = new PixelDisplayDialog(models);
            models.Window.ShowDialog(dia);
        }
    }
}
