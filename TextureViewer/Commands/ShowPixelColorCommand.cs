using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Views;

namespace TextureViewer.Commands
{
    public class ShowPixelColorCommand : ICommand
    {
        private Models.Models models;

        public ShowPixelColorCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var colors = new List<PixelColorDialog.Element>();
            for(int i = 0; i < models.Equations.NumEquations; ++i)
            {
                if(models.Equations.Get(i).Visible)
                {
                    colors.Add(new PixelColorDialog.Element(models.Equations.Get(i).TexelColor, i));
                }
            }

            if(colors.Count > 0)
            {
                var dia = new PixelColorDialog(colors);
                dia.Owner = models.App.Window;
                dia.ShowDialog();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
