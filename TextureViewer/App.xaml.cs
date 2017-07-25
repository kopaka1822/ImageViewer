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
        private LayerWindow layerWindow = null;
        private MipMapWindow mipMapWindow = null;

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

                    var wnd = new MainWindow(this, image);
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
                var wnd = new MainWindow(this, null);
                wnd.Show();
                activeWindows.Add(wnd);
            }
        }

        public void UnregisterWindow(MainWindow window)
        {
            activeWindows.Remove(window);

            if(activeWindows.Count == 0)
                Shutdown(0);
        }

        public void OpenLayerWindow()
        {
            if (layerWindow == null)
            {
                layerWindow = new LayerWindow(this);
                layerWindow.Show();
            }
            layerWindow.Focus();
        }

        public void OpenMipMapWindow()
        {
            if (mipMapWindow == null)
            {
                mipMapWindow = new MipMapWindow(this);
                mipMapWindow.Show();
            }
            mipMapWindow.Focus();
        }

        public void CloseLayerWindow()
        {
            if (layerWindow != null)
            {
                if(!layerWindow.IsClosing)
                    layerWindow.Close();
                layerWindow = null;
            }
        }

        public void CloseMipMapWindow()
        {
            if (mipMapWindow != null)
            {
                if(!mipMapWindow.IsClosing)
                    mipMapWindow.Close();
                mipMapWindow = null;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
