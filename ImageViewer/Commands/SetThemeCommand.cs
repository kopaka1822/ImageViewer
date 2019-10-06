using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Models;
using ImageViewer.Views.Theme;

namespace ImageViewer.Commands
{
    public class SetThemeCommand : SimpleCommand<int>
    {
        private readonly ModelsEx models;

        public SetThemeCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute(int parameter)
        {
            models.Settings.Theme = (ThemeDictionary.Themes) parameter;
        }
    }
}
