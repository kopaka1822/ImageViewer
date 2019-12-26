using ImageViewer.Commands.Helper;
using ImageViewer.Controller;
using ImageViewer.Models;

namespace ImageViewer.Commands.Import
{
    public class ImportCommand : SimpleCommand
    {
        private readonly ModelsEx models;
        private readonly ImportDialogController importDialog;

        public ImportCommand(ModelsEx models)
        {
            this.models = models;
            importDialog = new ImportDialogController(models);
        }

        public override async void Execute()
        {
            var files = importDialog.ShowImportImageDialog();
            if (files == null) return;

            foreach (var file in files)
            {
                await importDialog.ImportImageAsync(file);
            }
        }
    }
}
