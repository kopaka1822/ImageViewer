using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageViewer.Models;

namespace ImageViewer.ViewModels
{
    public class ProgressViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;

        public ProgressViewModel(ModelsEx models)
        {
            this.models = models;
            this.models.Progress.PropertyChanged += ProgressOnPropertyChanged;
            //this.models.Export.PropertyChanged += ExportOnPropertyChanged;
        }

        //private void ExportOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        //{
        //    switch (args.PropertyName)
        //    {
        //        case nameof(Models.Dialog.ExportModel.IsExporting):
        //            OnPropertyChanged(nameof(NotProcessing));
        //            break;
        //    }
        //}

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
                case nameof(ProgressModel.What):
                    OnPropertyChanged(nameof(ProgressDescription));
                    break;
            }
        }

        public Visibility EnableProgress => models.Progress.IsProcessing ? Visibility.Visible : Visibility.Collapsed;
        public bool NotProcessing => !models.Progress.IsProcessing;// && !models.Export.IsExporting;

        public float ProgressValue
        {
            get => models.Progress.Progress * 100.0f;
            // dont allow changes from the ui
            set => OnPropertyChanged(nameof(ProgressValue));
        }

        public string ProgressDescription => models.Progress.What;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
