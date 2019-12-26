using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands
{
    public class HelpDialogCommand : SimpleCommand<string>
    {
        private readonly ModelsEx models;

        public HelpDialogCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute(string parameter)
        {
            var dia = new HelpDialog(models, parameter);

            if(dia.IsValid)
                models.Window.ShowDialog(dia);
        }
    }
}
