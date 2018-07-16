using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class ProgressViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public ProgressViewModel(Models.Models models)
        {
            this.models = models;
            this.models.Progress.PropertyChanged += ProgressOnPropertyChanged;
        }

        private void ProgressOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ProgressModel.IsProcessing):
                    OnPropertyChanged(nameof(EnableProgress));
                    OnPropertyChanged(nameof(NotProcessing));
                    break;
                case nameof(ProgressModel.Progress):
                    OnPropertyChanged(nameof(ProgressValue));
                    break;
            }
        }

        public Visibility EnableProgress => models.Progress.IsProcessing ? Visibility.Visible : Visibility.Collapsed;
        public bool NotProcessing => !models.Progress.IsProcessing;

        public float ProgressValue
        {
            get => models.Progress.Progress * 100.0f;
            // dont allow changes from the ui
            set => OnPropertyChanged(nameof(ProgressValue));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
