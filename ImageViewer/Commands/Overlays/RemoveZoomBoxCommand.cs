using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Tools
{
    public class RemoveZoomBoxCommand : SimpleCommand<int>
    {
        private readonly ModelsEx models;

        public RemoveZoomBoxCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute(int id)
        {
            models.ZoomBox.Boxes.RemoveAt(id);
        }
    }
}
