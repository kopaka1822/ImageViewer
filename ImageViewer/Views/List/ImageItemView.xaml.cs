using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ImageViewer.Views.List
{
    /// <summary>
    /// Interaction logic for ImageItemView.xaml
    /// </summary>
    public partial class ImageItemView : ListBoxItem, INotifyPropertyChanged
    {
        private readonly ImagesModel.ImageData imgData;

        public ImageItemView(ImagesModel.ImageData imgData, int id, ImagesModel images)
        {
            InitializeComponent();

            this.imgData = imgData;
            Id = id;
            Prefix = $"I{id} - ";
            ToolTip = imgData.Filename + "\n" + imgData.OriginalFormat;
            if (imgData.Alias.StartsWith("__imported"))
            {
                imageName = "";
                imgData.Alias = "";
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke((Action)OnRename);
            }
            else imageName = imgData.Alias;

            DeleteCommand = new ActionCommand(() => images.DeleteImage(id));
            RenameCommand = new ActionCommand(OnRename);
            TextInputBox.LostFocus += TextInputBoxOnLostFocus;

            DataContext = this;
        }

        public int Id { get; }

        public string Prefix { get; }

        private string imageName;

        public string ImageName
        {
            get => imageName;
            set
            {
                if (value == null || value == imageName) return;
                imageName = value;
                OnPropertyChanged(nameof(ImageName));
                IsRenaming = false;
                imgData.Alias = imageName;
            }
        }

        private bool isRenaming = false;

        public bool IsRenaming
        {
            get => isRenaming;
            set
            {
                if(value == isRenaming) return;
                isRenaming = value;
                OnPropertyChanged(nameof(IsRenaming));
            }
        }

        public ICommand DeleteCommand { get; }
        public ICommand RenameCommand { get; }

        private async void OnRename()
        {
            IsRenaming = true;
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
            IsRenaming = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NameMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                //OnRename();
                //e.Handled = false;
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke((Action) OnRename);
            }
        }
    }
}
