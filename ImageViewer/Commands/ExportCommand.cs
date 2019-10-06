using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Models;

namespace ImageViewer.Commands
{
    public class ExportCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public ExportCommand(ModelsEx models)
        {
            this.models = models;
        }


        public override void Execute()
        {
            if (models.Images.NumImages == 0) return;

            // make sure only one image is visible
            throw new NotImplementedException();
        }
    }
}
