using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow win1;
        private MainWindow win2;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            win1 = new MainWindow();
            win1.Show();
            win2 = new MainWindow();
            win2.Show();
        }
    }
}
