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
        public enum UniqueDialog
        {
            Layer,
            Mipmaps,
            Image
        }


        private List<MainWindow> openWindows = new List<MainWindow>();
        
        private MainWindow activeWindow = null; // the last window that was active
        private Dictionary<UniqueDialog, Window> uniqueDialogs = new Dictionary<UniqueDialog, Window>();

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

        public void OpenDialog(UniqueDialog dialog)
        {
            Window window;
            if (!uniqueDialogs.TryGetValue(dialog, out window))
            {
                // add dialog
                switch (dialog)
                {
                    case UniqueDialog.Layer:
                        window = new LayerWindow(this);
                        break;
                    case UniqueDialog.Mipmaps:
                        window = new MipMapWindow(this);
                        break;
                    case UniqueDialog.Image:
                        window = new ImageWindow(this);
                        break;
                }
                IUniqueDialog dia = (IUniqueDialog) window;
                dia?.UpdateContent(activeWindow);
                window?.Show();
                
                if(window != null)
                    uniqueDialogs.Add(dialog, window);
            }
            window?.Focus();
        }

        public void CloseDialog(UniqueDialog dialog)
        {
            Window window;
            if (uniqueDialogs.TryGetValue(dialog, out window))
            {
                IUniqueDialog dia = window as IUniqueDialog;
                if (dia != null)
                {
                    if(!dia.IsClosing)
                        window.Close();
                }
                uniqueDialogs.Remove(dialog);
            }
        }

        public void SetActiveWindow(MainWindow window)
        {
            // assert that it was registered
            Debug.Assert(openWindows.IndexOf(window) >= 0);
            activeWindow = window;
            window.ZIndex = lastZIndex++;

            // refresh dialogs
            foreach (var w in uniqueDialogs)
            {
                IUniqueDialog dia = w.Value as IUniqueDialog;
                dia?.UpdateContent(activeWindow);
            }

            UpdateDialogVisibility();
        }

        public void UpdateDialogVisibility()
        {
            bool topmost = false;
            foreach (var openWindow in openWindows)
            {
                if (openWindow.IsActive)
                {
                    topmost = true;
                    break;
                }
            }

            foreach (var w in uniqueDialogs)
                w.Value.Topmost = topmost;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
