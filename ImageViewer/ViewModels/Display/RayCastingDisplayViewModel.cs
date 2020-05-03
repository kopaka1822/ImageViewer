using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.UtilityEx;

namespace ImageViewer.ViewModels.Display
{
    public class RayCastingDisplayViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly RayCastingDisplayModel displayViewEx;
        private readonly ModelsEx models;

        public RayCastingDisplayViewModel(ModelsEx models)
        {
            this.models = models;
            this.displayViewEx = (RayCastingDisplayModel)models.Display.ExtendedViewData;

            displayViewEx.PropertyChanged += DisplayViewExOnPropertyChanged;
            models.Display.PropertyChanged += DisplayOnPropertyChanged;
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
            Crop = models.ExportConfig.GetViewModel(models);
            Crop.Mipmap = models.Display.ActiveMipmap;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.Size):
                    // refresh crop thing
                    Crop.Dispose();
                    Crop = models.ExportConfig.GetViewModel(models);
                    OnPropertyChanged(nameof(Crop));
                    break;
            }
        }

        public void Dispose()
        {
            Crop.Dispose();
            models.Display.PropertyChanged -= DisplayOnPropertyChanged;
            models.Images.PropertyChanged -= ImagesOnPropertyChanged;
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.LinearInterpolation):
                    OnPropertyChanged(nameof(FlatIsEnabled));
                    OnPropertyChanged(nameof(FlatShading));
                    OnPropertyChanged(nameof(AlphaIsCoverage));
                    OnPropertyChanged(nameof(HideInternals));
                    break;
                case nameof(DisplayModel.ActiveMipmap):
                    Crop.Mipmap = models.Display.ActiveMipmap;
                    break;
            }
        }

        private void DisplayViewExOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(RayCastingDisplayModel.FlatShading):
                    OnPropertyChanged(nameof(FlatShading));
                    break;
                case nameof(RayCastingDisplayModel.UseCropping):
                    OnPropertyChanged(nameof(UseCropping));
                    break;
                case nameof(RayCastingDisplayModel.SelfShadowing):
                    OnPropertyChanged(nameof(SelfShadowing));
                    break;
                case nameof(RayCastingDisplayModel.AlphaIsCoverage):
                    OnPropertyChanged(nameof(AlphaIsCoverage));
                    break;
                case nameof(RayCastingDisplayModel.HideInternals):
                    OnPropertyChanged(nameof(HideInternals));
                    break;
            }
        }

        public bool FlatShading
        {
            get => !models.Display.LinearInterpolation && displayViewEx.FlatShading;
            set => displayViewEx.FlatShading = value;
        }

        public bool FlatIsEnabled => !models.Display.LinearInterpolation;

        public bool UseCropping
        {
            get => displayViewEx.UseCropping;
            set => displayViewEx.UseCropping = value;
        }

        public bool SelfShadowing
        {
            get => displayViewEx.SelfShadowing;
            set => displayViewEx.SelfShadowing = value;
        }

        public bool AlphaIsCoverage
        {
            get => models.Display.LinearInterpolation || displayViewEx.AlphaIsCoverage;
            set => displayViewEx.AlphaIsCoverage = value;
        }

        public bool HideInternals
        {
            get => !models.Display.LinearInterpolation && displayViewEx.HideInternals;
            set => displayViewEx.HideInternals = value;
        }

        public CropManager.ViewModel Crop { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
