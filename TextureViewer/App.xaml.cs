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
                // create empty window
                var wnd = new MainWindow(null);
                wnd.Show();
                activeWindows.Add(wnd);
                //this.Shutdown(0);
            }
            else
            {
                // open a window for each file
                foreach (var s in e.Args)
                {
                    var wnd = new MainWindow(s);
                    wnd.Show();
                    activeWindows.Add(wnd);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
