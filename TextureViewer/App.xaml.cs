using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
    }
}
