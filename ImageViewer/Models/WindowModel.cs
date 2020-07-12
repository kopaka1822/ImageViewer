using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageViewer.DirectX;
using ImageViewer.Views.Dialog;
using Size = System.Drawing.Size;

namespace ImageViewer.Models
{
    public class WindowModel : INotifyPropertyChanged, IDisposable
    {
        public MainWindow Window { get; }

        public Window TopmostWindow => windowStack.Peek();

        private readonly Stack<Window> windowStack = new Stack<Window>();
        public Size ClientSize { get; private set; } = new Size(0, 0);

        public string ExecutionPath { get; }
        public string AssemblyPath { get; }

        public ImageFramework.Utility.Color ThemeColor { get; }

        public SwapChain SwapChain { get; private set; } = null;
        public WindowModel(MainWindow window)
        {
            Window = window;
            AssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            ExecutionPath = System.IO.Path.GetDirectoryName(AssemblyPath);

            window.Loaded += WindowOnLoaded;

            windowStack.Push(window);

            var bgBrush = (SolidColorBrush)Window.FindResource("BackgroundBrush");
            var tmpColor = new ImageFramework.Utility.Color(bgBrush.Color.ScR, bgBrush.Color.ScG, bgBrush.Color.ScB, 1.0f);
            ThemeColor = tmpColor.ToSrgb();
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
            ClientSize = new Size(
                (int)(Window.BorderHost.ActualWidth),
                (int)(Window.BorderHost.ActualHeight)
            );
            OnPropertyChanged(nameof(ClientSize));

            // change size event
            Window.BorderHost.SizeChanged += (sender2, args2) =>
            {
                ClientSize = new Size(
                    (int)(Window.BorderHost.ActualWidth), 
                    (int)(Window.BorderHost.ActualHeight)
                );
                OnPropertyChanged(nameof(ClientSize));
            };

            var adapter = new SwapChainAdapter(Window.BorderHost);
            Window.BorderHost.Child = adapter;
            SwapChain = adapter.SwapChain;

            SwapChain.Resize(ClientSize.Width, ClientSize.Height);
            OnPropertyChanged(nameof(SwapChain));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowErrorDialog(string message, string where = "")
        {
            where = String.IsNullOrEmpty(where) ? "Error" : "Error " + where;

#if DEBUG
            var res = MessageBoxResult.None;
            message += ". Do you want to debug the application?";          
            res = MessageBox.Show(TopmostWindow, message, where, MessageBoxButton.YesNo, MessageBoxImage.Error);
            
            if (res == MessageBoxResult.Yes)
                Debugger.Break();
#else
            
            MessageBox.Show(TopmostWindow, message, where, MessageBoxButton.OK, MessageBoxImage.Error);
#endif
        }

        public void ShowErrorDialog(Exception exception, string where = "")
        {
            if (exception is Shader.CompilationException ce)
            {
                var dia = new ShaderExceptionDialog(ce.Error, ce.Code);
                dia.Title = $"Shader Error in {ce.Name}";

                ShowDialog(dia);
            }
            else ShowErrorDialog(exception.Message, where);
        }

        public void ShowInfoDialog(string message, string title = "Info")
        {
            MessageBox.Show(TopmostWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowYesNoDialog(string message, string title)
        {
            return MessageBox.Show(TopmostWindow, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question,
                       MessageBoxResult.Yes) == MessageBoxResult.Yes;
        }

        public void Dispose()
        {
            SwapChain?.Dispose();
        }
    }
}
