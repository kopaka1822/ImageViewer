using System.ComponentModel;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.Import
{
    public class ImportArrayCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public ImportArrayCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute()
        {
            using (var viewModel = new ImportArrayViewModel(models))
            {
                var dia = new ArrayImportDialog { DataContext = viewModel };
                models.Window.ShowDialog(dia);
            }
        }
    }
}
