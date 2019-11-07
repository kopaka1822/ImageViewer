using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands
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
                if (models.Window.ShowDialog(dia) != true) return;

                var textures = viewModel.GetTextures();

                var res = models.CombineToArray(textures);
                models.Images.AddImage(res, "TextureArray", GliFormat.UNDEFINED);
            }
        }
    }
}
