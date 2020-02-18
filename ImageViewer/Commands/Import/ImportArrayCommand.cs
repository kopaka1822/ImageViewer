using System.ComponentModel;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.Import
{
    public class ImportArrayCommand : Command
    {
        private readonly ModelsEx models;

        public ImportArrayCommand(ModelsEx models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.ImageType):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return  models.Images.ImageType != typeof(Texture3D);
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
