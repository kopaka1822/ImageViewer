using ImageViewer.Commands.Helper;
using ImageViewer.Controller;
using ImageViewer.Models;

namespace ImageViewer.Commands.Import
{
    public class OpenCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public OpenCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override async void Execute()
        {
            var files = models.Import.ShowImportImageDialog();
            if (files == null) return;

            // nothing imported yet => show in same window
            if (models.Images.NumImages == 0)
            {
                foreach (var file in files)
                {
                    await models.Import.ImportImageAsync(file);
                }
                return;
            }

            // already images imported => show in new window
            var args = "";
            foreach (var file in files)
            {
                args += $"\"{file}\" ";
            }

            var info = new System.Diagnostics.ProcessStartInfo(models.Window.AssemblyPath, args);
            System.Diagnostics.Process.Start(info);
        }
    }
}
