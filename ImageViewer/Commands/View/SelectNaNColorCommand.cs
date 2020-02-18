using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.View
{
    public class SelectNaNColorCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public SelectNaNColorCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute()
        {
            var cp = new ColorPickerDialog(models.Settings.NaNColor);

            if (models.Window.ShowDialog(cp) != true) return;

            models.Settings.NaNColor = cp.Color;
        }
    }
}
