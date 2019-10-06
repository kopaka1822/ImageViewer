using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageViewer
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
        public static readonly string AppName = "ImageViewer";
        private static readonly Dictionary<ResourceIcon, BitmapImage> icons = new Dictionary<ResourceIcon, BitmapImage>();
        public static string[] StartupArgs { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            StartupArgs = e.Args;

            base.OnStartup(e);

            LoadIcons();
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

        public static BitmapImage GetResourceImage(ResourceIcon r)
        {
            return icons[r];
        }
    }
}
