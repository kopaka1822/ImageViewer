using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models
{
    /// <summary>
    /// application releveant data
    /// </summary>
    public class AppModel
    {
        public App App { get; }
        public MainWindow Window { get; }

        public AppModel(App app, MainWindow window)
        {
            App = app;
            Window = window;
        }
    }
}
