using ImageViewer.Commands.Helper;
using ImageViewer.Controller;
using ImageViewer.Models;

namespace ImageViewer.Commands.Import
{
    public class ImportCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public ImportCommand(ModelsEx models)
        {
            this.models = models;
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
