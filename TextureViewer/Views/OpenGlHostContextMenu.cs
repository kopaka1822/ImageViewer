using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TextureViewer.ViewModels;

namespace TextureViewer.Views
{
    public class OpenGlHostContextMenu : System.Windows.Forms.ContextMenuStrip
    {
        public OpenGlHostContextMenu(WindowViewModel viewModel)
        {
            var colorItem = Items.Add("Pixel Color");
            {
                var sri = System.Windows.Application.GetResourceStream(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/eyedropper.png", UriKind.Absolute));
                if (sri != null)
                    colorItem.Image = System.Drawing.Image.FromStream(sri.Stream);

                colorItem.Click += (o, args) =>
                {
                    viewModel.ShowPixelColorCommand.Execute(null);
                };
            }

            var pixelDisplayItem = Items.Add("Pixel Display");
            {
                var sri = System.Windows.Application.GetResourceStream(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/displayconfig.png", UriKind.Absolute));
                if (sri != null)
                    pixelDisplayItem.Image = System.Drawing.Image.FromStream(sri.Stream);

                pixelDisplayItem.Click += (o, args) =>
                {
                    viewModel.ShowPixelDisplayCommand.Execute(null);
                };
            }
        }
    }
}
