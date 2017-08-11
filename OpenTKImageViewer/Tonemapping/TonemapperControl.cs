using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Tonemapping
{
    public class TonemapperControl
    {
        private MainWindow window;

        private class Tonemapper
        {
            private ShaderLoader loader;
            private ToneShader shader;

            public Tonemapper(string filename)
            {
                loader = new ShaderLoader(filename);
                shader = new ToneShader(loader);
            }
        }

        private List<Tonemapper> tonemappers = new List<Tonemapper>();

        public TonemapperControl(MainWindow window)
        {
            this.window = window;
        }

        public void AddTonemapper(string filename)
        {
            try
            {
                // try to open the file
                var mapper = new Tonemapper(filename);
                tonemappers.Add(mapper);
            }
            catch (Exception e)
            {
                App.ShowErrorDialog(window, e.Message);
            }
        }
    }
}
