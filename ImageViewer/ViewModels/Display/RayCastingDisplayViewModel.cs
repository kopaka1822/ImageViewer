using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageViewer.Models;
using ImageViewer.Models.Display;

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
        }

        public void Dispose()
        {
            models.Display.PropertyChanged -= DisplayOnPropertyChanged;
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.LinearInterpolation):
                    OnPropertyChanged(nameof(FlatIsEnabled));
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
            }
        }

        public bool FlatShading
        {
            get => displayViewEx.FlatShading;
            set => displayViewEx.FlatShading = value;
        }

        public bool FlatIsEnabled => !models.Display.LinearInterpolation;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
