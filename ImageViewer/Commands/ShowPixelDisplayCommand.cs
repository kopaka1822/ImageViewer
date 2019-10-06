using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Models;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands
{
    public class ShowPixelDisplayCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public ShowPixelDisplayCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute()
        {
            var dia = new PixelDisplayDialog(models);
            models.Window.ShowDialog(dia);
        }
    }
}
