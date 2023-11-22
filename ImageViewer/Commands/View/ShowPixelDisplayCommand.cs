using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.View
{
    public class ShowPixelDisplayCommand : SimpleCommand
    {
        public ShowPixelDisplayCommand(ModelsEx models) : base(models)
        {
        }

        public override void Execute()
        {
            var dia = new PixelDisplayDialog(models);
            models.Window.ShowDialog(dia);
        }
    }
}
