using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageViewer.Commands;
using ImageViewer.Commands.Helper;
using ImageViewer.ViewModels.Image;

namespace ImageViewer.Views.List
{
    /// <summary>
    /// Interaction logic for ImageItemView.xaml
    /// </summary>
    public partial class ImageItemView : UserControl
    {
        public ImageItemViewModel ViewModel { get; private set; }

        public ImageItemView()
        {
            InitializeComponent();

            TextInputBox.LostFocus += TextInputBoxOnLostFocus;
        }

        private async void OnRename()
        {
            TextInputBox.SelectAll();

            int count = 0;
            // try to get element focus
            while (!TextInputBox.Focus() && ++count < 20)
            {
                await Task.Delay(1);
            }
        }

        private void TextInputBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.IsRenaming = false;
        }

        private void NameMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() =>
                {
                    if (ViewModel != null)
                        ViewModel.IsRenaming = true;
                }));
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // unsubscribe old
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= DataContextOnPropertyChanged;
            }

            if (DataContext == null)
            {
                ViewModel = null;
                return;
            }

            // subscribe new
            Debug.Assert(DataContext is ImageItemViewModel);
            ViewModel = (ImageItemViewModel) DataContext;
            ViewModel.PropertyChanged += DataContextOnPropertyChanged;
        }

        private void DataContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImageItemViewModel.IsRenaming):
                    if(ViewModel.IsRenaming)
                        OnRename();
                    break;
            }
        }
    }
}
