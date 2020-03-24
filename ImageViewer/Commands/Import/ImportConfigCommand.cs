using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Import
{
    public class ImportConfigCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public ImportConfigCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute()
        {
            

        }
    }
}
