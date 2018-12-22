using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public enum ResourceIcon
        {
            Cancel,
            Eye,
            EyeClosed,
            ListMove
        }

        // change this if the assembly name was changed
        public static readonly string AppName = "TextureViewer";

        public static readonly int MaxImageViews = 4;
        public string ExecutionPath { get; private set; }

        private readonly List<MainWindow> openWindows = new List<MainWindow>();
        private static readonly Dictionary<ResourceIcon, BitmapImage> icons = new Dictionary<ResourceIcon, BitmapImage>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ExecutionPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            LoadIcons();

            if (e.Args.Length == 0)
            {
                // open empty window
                SpawnWindow();
            }
            else
            {
                // import images into one window
                var wnd = SpawnWindow();
                wnd.ImportImages(e.Args);
            }
        }

        private static void LoadIcons()
        {
            icons[ResourceIcon.Cancel] =
                new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/cancel.png",
                    UriKind.Absolute));

            icons[ResourceIcon.Eye] =
                new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/eye.png",
                    UriKind.Absolute));

            icons[ResourceIcon.EyeClosed] =
                new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/eye_closed.png",
                    UriKind.Absolute));

            icons[ResourceIcon.ListMove] = 
                new BitmapImage(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/list_move.png",
                    UriKind.Absolute));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TextureViewer.Properties.Settings.Default.Save();

            base.OnExit(e);
        }

        public MainWindow SpawnWindow()
        {
            var wnd = new MainWindow(this);
            openWindows.Add(wnd);

            wnd.Show();

            wnd.Closed += (sender, args) => openWindows.Remove(wnd);

            return wnd;
        }

        /// <summary>
        /// displays error dialog (with debug option in debug mode)
        /// </summary>
        /// <param name="owner">parent of the dialog (may be null)</param>
        /// <param name="message">error message</param>
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

        /// <summary>
        /// displays information in a message box
        /// </summary>
        /// <param name="owner">parent of the dialoge (may be null)</param>
        /// <param name="message">information message</param>
        public static void ShowInfoDialog(Window owner, string message)
        {
            if (owner != null)
                MessageBox.Show(owner, message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool ShowYesNoDialog(Window owner, string title, string message)
        {
            if (owner != null)
                return MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes;
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes;
        }

        private static readonly CultureInfo CultureInfo = new CultureInfo("en-US");

        public static CultureInfo GetCulture()
        {
            return CultureInfo;
        }

        public static BitmapImage GetResourceImage(ResourceIcon r)
        {
            return icons[r];
        }
    }
}