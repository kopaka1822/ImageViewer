using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model.Scaling;
using ImageViewer.Models;

namespace ImageViewer.ViewModels
{
    public class ScalingViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;

        public ScalingViewModel(ModelsEx models)
        {
            this.models = models;
            models.Scaling.PropertyChanged += ScalingOnPropertyChanged;
        }

        private void ScalingOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ScalingModel.Minify):
                    OnPropertyChanged(nameof(UseBoxFilter));
                    OnPropertyChanged(nameof(UseTriangleFilter));
                    OnPropertyChanged(nameof(UseLanzosFilter));
                    OnPropertyChanged(nameof(UseDetailPreserving));
                    OnPropertyChanged(nameof(UseVeryDetailPreserving));
                    break;
            }
        }

        public bool UseBoxFilter
        {
            get => models.Scaling.Minify == ScalingModel.MinifyFilters.Box;
            set => SetMinify(value, ScalingModel.MinifyFilters.Box);
        }

        public bool UseTriangleFilter
        {
            get => models.Scaling.Minify == ScalingModel.MinifyFilters.Triangle;
            set => SetMinify(value, ScalingModel.MinifyFilters.Triangle);
        }

        public bool UseLanzosFilter
        {
            get => models.Scaling.Minify == ScalingModel.MinifyFilters.Lanzos;
            set => SetMinify(value, ScalingModel.MinifyFilters.Lanzos);
        }

        public bool UseDetailPreserving
        {
            get => models.Scaling.Minify == ScalingModel.MinifyFilters.DetailPreserving;
            set => SetMinify(value, ScalingModel.MinifyFilters.DetailPreserving);
        }

        public bool UseVeryDetailPreserving
        {
            get => models.Scaling.Minify == ScalingModel.MinifyFilters.VeryDetailPreserving;
            set => SetMinify(value, ScalingModel.MinifyFilters.VeryDetailPreserving);
        }

        private void SetMinify(bool value, ScalingModel.MinifyFilters filter)
        {
            if (value) models.Scaling.Minify = filter;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
