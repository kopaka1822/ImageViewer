using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Tools
{
    public class RemoveArrowCommand : SimpleCommand<int>
    {
        public RemoveArrowCommand(ModelsEx models) : base(models)
        {
        }

        public override void Execute(int id)
        {
            models.Arrows.Arrows.RemoveAt(id);
        }
    }
}
