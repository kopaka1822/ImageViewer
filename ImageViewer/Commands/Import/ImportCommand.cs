using ImageViewer.Commands.Helper;
using ImageViewer.Controller;
using ImageViewer.Models;

namespace ImageViewer.Commands.Import
{
    public class ImportCommand : SimpleCommand
    {
        public ImportCommand(ModelsEx models) : base(models)
        {
        }

        public override async void Execute()
        {
            var files = models.Import.ShowImportImageDialog();
            if (files == null) return;

            foreach (var file in files)
            {
                await models.Import.ImportFileAsync(file);
            }
        }
    }
}
