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
using SharpDX.Direct3D11;
using SharpDX.DXGI;

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
                case nameof(ScalingModel.AlphaTestProcess):
                    OnPropertyChanged(nameof(UseAlphaNone));
                    OnPropertyChanged(nameof(UseAlphaScale));
                    OnPropertyChanged(nameof(UseAlphaPyramid));
                    OnPropertyChanged(nameof(UseAlphaConnectivity));
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
            get => models.Scaling.Minify == ScalingModel.MinifyFilters.Lanczos;
            set => SetMinify(value, ScalingModel.MinifyFilters.Lanczos);
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

        public bool UseAlphaNone
        {
            get => models.Scaling.AlphaTestProcess == ScalingModel.AlphaTestPostprocess.None;
            set => SetAlpha(value, ScalingModel.AlphaTestPostprocess.None);
        }

        public bool UseAlphaScale
        {
            get => models.Scaling.AlphaTestProcess == ScalingModel.AlphaTestPostprocess.AlphaScale;
            set => SetAlpha(value, ScalingModel.AlphaTestPostprocess.AlphaScale);
        }

        public bool UseAlphaPyramid
        {
            get => models.Scaling.AlphaTestProcess == ScalingModel.AlphaTestPostprocess.AlphaPyramid;
            set => SetAlpha(value, ScalingModel.AlphaTestPostprocess.AlphaPyramid);
        }

        public bool UseAlphaConnectivity
        {
            get => models.Scaling.AlphaTestProcess == ScalingModel.AlphaTestPostprocess.AlphaConnectivity;
            set => SetAlpha(value, ScalingModel.AlphaTestPostprocess.AlphaConnectivity);
        }

        private void SetAlpha(bool value, ScalingModel.AlphaTestPostprocess mode)
        {
            if(value) models.Scaling.AlphaTestProcess = mode;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
