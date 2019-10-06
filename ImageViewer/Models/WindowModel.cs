using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.Annotations;
using Size = System.Drawing.Size;

namespace ImageViewer.Models
{
    public class WindowModel : INotifyPropertyChanged
    {
        public MainWindow Window { get; }
        public Window TopmostWindow => windowStack.Peek();

        private readonly Stack<Window> windowStack = new Stack<Window>();
        public Size ClientSize { get; private set; } = new Size(0, 0);

        public string ExecutionPath { get; }
        public string AssemblyPath { get; }

        public WindowModel(MainWindow window)
        {
            Window = window;
            AssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            ExecutionPath = System.IO.Path.GetDirectoryName(AssemblyPath);

            window.Loaded += WindowOnLoaded;
            window.BorderHost.DragOver += (o, args) => args.Effects = DragDropEffects.Copy;
            window.BorderHost.AllowDrop = true;

            windowStack.Push(window);
        }

        /// <summary>
        /// shows a modal dialog and sets the correct dialog owner (topmost window)
        /// </summary>
        /// <returns>result of the dialog</returns>
        public bool? ShowDialog(Window dialog)
        {
            dialog.Owner = TopmostWindow;
            windowStack.Push(dialog);
            var res = dialog.ShowDialog();
            windowStack.Pop();
            return res;
        }

        private void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            // set initial client size dimensions
            var source = PresentationSource.FromVisual(Window);
            var scalingX = source.CompositionTarget.TransformToDevice.M11;
            var scalingY = source.CompositionTarget.TransformToDevice.M22;
            var w = (int) (Window.BorderHost.ActualWidth * scalingX);
            var h = (int) (Window.BorderHost.ActualHeight * scalingY);

            ClientSize = new Size(w, h);
            OnPropertyChanged(nameof(ClientSize));

            // change size event
            Window.BorderHost.SizeChanged += (sender2, args2) =>
            {
                ClientSize = new Size(
                    (int)(Window.BorderHost.ActualWidth * scalingX), 
                    (int)(Window.BorderHost.ActualHeight * scalingY)
                );
                OnPropertyChanged(nameof(ClientSize));
            };
        }

        private bool issuedRedraw = false;

        /// <summary>
        /// the frame will be redrawn as soon as possible
        /// </summary>
        public void RedrawFrame()
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowErrorDialog(string message)
        {
#if DEBUG
            var res = MessageBoxResult.None;
            message += ". Do you want to debug the application?";          
            res = MessageBox.Show(TopmostWindow, message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            
            if (res == MessageBoxResult.Yes)
                Debugger.Break();
#else
            
            MessageBox.Show(TopmostWindow, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
        }
    }
}
