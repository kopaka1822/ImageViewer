using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using OpenTKImageViewer.Dialogs;
using OpenTKImageViewer.Utility;

namespace OpenTKImageViewer
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
        private Settings appSettings = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            appSettings = new Settings(Environment.CurrentDirectory + "/config.json");

            if (e.Args.Length == 0)
            {
                SpawnWindow((string)null);
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

        protected override void OnExit(ExitEventArgs e)
        {
            appSettings.Save();
            base.OnExit(e);
        }

        public static void ShowErrorDialog(Window owner, string message)
        {
#if DEBUG
            var res = MessageBoxResult.None;
            message += ". Do you want to debug the application?";
            if (owner != null)
                res = MessageBox.Show(owner, message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            else
                res = MessageBox.Show(message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (res == MessageBoxResult.Yes)
                Debugger.Break();
#else
            if(owner != null)
                MessageBox.Show(owner, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif

        }

        public static void ShowInfoDialog(Window owner, string message)
        {
            if (owner != null)
                MessageBox.Show(owner, message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void SpawnWindow(string filename)
        {
            if (filename != null)
            {
                // try to load the resource
                try
                {
                    var images = ImageLoader.LoadImage(filename);

                    SpawnWindow(images);
                }
                catch (Exception e)
                {
                    // could not load the image
                    ShowErrorDialog(null, e.Message);
                }
            }

            if (openWindows.Count == 0)
            {
                // spawn empty window
                var wnd = new MainWindow(this, new ImageContext.ImageContext(null));
                activeWindow = wnd;
                openWindows.Add(wnd);

                wnd.Show();
            }
        }

        /// <summary>
        /// opens a window with the provided images
        /// </summary>
        /// <param name="images"></param>
        public void SpawnWindow(List<ImageLoader.Image> images)
        {
            var wnd = new MainWindow(this, new ImageContext.ImageContext(images));
            activeWindow = wnd;
            openWindows.Add(wnd);

            wnd.Show();

            // open relevant dialoges
            if (wnd.Context.GetNumImages() > 1)
                OpenDialog(UniqueDialog.Image);
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

        public MainWindow GetActiveWindow()
        {
            return activeWindow;
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
                IUniqueDialog dia = (IUniqueDialog)window;
                dia?.UpdateContent(activeWindow);
                window?.Show();

                if (window != null)
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
                    if (!dia.IsClosing)
                        window.Close();
                }
                uniqueDialogs.Remove(dialog);
            }
        }

        /// <summary>
        /// gets the default path for image files
        /// </summary>
        /// <param name="fd"></param>
        /// <returns></returns>
        public string GetImagePath(FileDialog fd)
        {
            if (appSettings.GetConfig().ImagePath.Length > 0)
            {
                return appSettings.GetConfig().ImagePath;
            }
            return fd.InitialDirectory;
        }

        /// <summary>
        /// sets the new path for image files depending on the selected one in the file dialog
        /// </summary>
        /// <param name="fd"></param>
        public void SetImagePath(FileDialog fd)
        {
            appSettings.GetConfig().ImagePath = System.IO.Path.GetDirectoryName(fd.FileName);
        }

        /// <summary>
        /// gets the default path for tonemapper shader files
        /// </summary>
        /// <param name="fd"></param>
        /// <returns></returns>
        public string GetShaderPath(FileDialog fd)
        {
            if (appSettings.GetConfig().TonemapperPath.Length > 0)
            {
                return appSettings.GetConfig().TonemapperPath;
            }
            return fd.InitialDirectory;
        }

        /// <summary>
        /// sets the new path for tonemapper shader files depending on the selected one in the file dialog
        /// </summary>
        /// <param name="fd"></param>
        public void SetShaderPath(FileDialog fd)
        {
            appSettings.GetConfig().TonemapperPath = System.IO.Path.GetDirectoryName(fd.FileName);
        }

        /// <summary>
        /// gets the default path for exporting files
        /// </summary>
        /// <param name="fd"></param>
        /// <returns></returns>
        public string GetExportPath(FileDialog fd)
        {
            if (appSettings.GetConfig().ExportPath.Length > 0)
            {
                return appSettings.GetConfig().ExportPath;
            }
            return fd.InitialDirectory;
        }

        /// <summary>
        /// sets the new path for exporting files depending on the selected one in the file dialog
        /// </summary>
        /// <param name="fd"></param>
        public void SetExportPath(FileDialog fd)
        {
            appSettings.GetConfig().ExportPath = System.IO.Path.GetDirectoryName(fd.FileName);
        }
    }
}
