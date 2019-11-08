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

                models.Images.AddImage(res, 
                    GetPrettyFilename(viewModel.ListItems[0].Filename, textures.Count), 
                    viewModel.ListItems[0].Format);
            }
        }

        private string GetPrettyFilename(string fullPath, int numTextures)
        {
            if (numTextures != 6) return fullPath;

            var filename = System.IO.Path.GetFileName(fullPath);
            if (filename == null) return fullPath;
            if (!filename.StartsWith("x")) return fullPath;

            // use directory name
            var path = System.IO.Path.GetDirectoryName(fullPath);
            if (path == null) return fullPath;
            var actualDir = System.IO.Path.GetFileName(path);
            if (actualDir.Length != 0) return actualDir;
            return fullPath;
        }
    }
}
