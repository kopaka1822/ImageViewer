using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private List<MainWindow> activeWindows = new List<MainWindow>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args.Length == 0)
            {
                SpawnWindow(@"E:\git\TextureViewer\TextureViewer\Crate.bmp");
            }
            else
            {
                // open a window for each file
                foreach (var s in e.Args)
                {
                    SpawnWindow(s);
                }
            }
        }

        private void SpawnWindow(string filename)
        {
            if (filename != null)
            {
                // try to load the resource
                try
                {
                    var image = ImageLoaderWrapper.LoadImage(filename);

                    var wnd = new MainWindow(image);
                    wnd.Show();
                    activeWindows.Add(wnd);
                }
                catch (Exception e)
                {
                    // could not load the image
                    // TODO delete this
                    MessageBox.Show(e.Message);
                }
            }

            if (activeWindows.Count == 0)
            {
                // spawn empty window
                var wnd = new MainWindow(null);
                wnd.Show();
                activeWindows.Add(wnd);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
