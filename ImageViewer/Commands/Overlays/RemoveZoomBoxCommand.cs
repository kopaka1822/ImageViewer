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
        public RemoveZoomBoxCommand(ModelsEx models) : base(models)
        {
        }

        public override void Execute(int id)
        {
            models.ZoomBox.Boxes.RemoveAt(id);
        }
    }
}
