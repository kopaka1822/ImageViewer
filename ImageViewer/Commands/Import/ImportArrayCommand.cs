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
        public ImportArrayCommand(ModelsEx models) : base(models)
        {
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
