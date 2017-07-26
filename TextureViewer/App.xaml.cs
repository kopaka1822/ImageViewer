using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
        private List<MainWindow> openWindows = new List<MainWindow>();
        private MainWindow activeWindow = null; // the last window that was active
        private LayerWindow layerWindow = null;
        private MipMapWindow mipMapWindow = null;
        private ulong lastZIndex = 1;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args.Length == 0)
            {
                SpawnWindow(null);
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

        public void SpawnWindow(string filename)
        {
            if (filename != null)
            {
                // try to load the resource
                try
                {
                    var image = ImageLoaderWrapper.LoadImage(filename);

                    var wnd = new MainWindow(this, image);
                    activeWindow = wnd;
                    openWindows.Add(wnd);

                    wnd.Show();
                }
                catch (Exception e)
                {
                    // could not load the image
                    // TODO delete this
                    MessageBox.Show(e.Message);
                }
            }

            if (openWindows.Count == 0)
            {
                // spawn empty window
                var wnd = new MainWindow(this, null);
                activeWindow = wnd;
                openWindows.Add(wnd);
                
                wnd.Show();
            }
        }

        public void UnregisterWindow(MainWindow window)
        {
            openWindows.Remove(window);

            if (openWindows.Count == 0)
            {
                Shutdown(0);
                return;
            }

            if (ReferenceEquals(activeWindow, window))
            {
                // set new active window (highest z index)
                window.ZIndex = 0;
                foreach (var w in openWindows)
                {
                    if (w.ZIndex > activeWindow.ZIndex)
                        activeWindow = w;
                }
                // this is actially the active window
                SetActiveWindow(activeWindow);
            }
        }

        public void OpenLayerWindow()
        {
            if (layerWindow == null)
            {
                layerWindow = new LayerWindow(this);
                layerWindow.UpdateContent(activeWindow);
                layerWindow.Show();
            }
            layerWindow.Focus();
        }

        public void OpenMipMapWindow()
        {
            if (mipMapWindow == null)
            {
                mipMapWindow = new MipMapWindow(this);
                mipMapWindow.UpdateContent(activeWindow);
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

        public void SetActiveWindow(MainWindow window)
        {
            // assert that it was registered
            Debug.Assert(openWindows.IndexOf(window) >= 0);
            activeWindow = window;
            window.ZIndex = lastZIndex++;

            // refresh dialogs
            mipMapWindow?.UpdateContent(activeWindow);
            layerWindow?.UpdateContent(activeWindow);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
