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
using ImageViewer.Views;

namespace ImageViewer.ViewModels.Display
{
    public class Single3DDisplayViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Single3DDisplayModel displayEx;
        private readonly ModelsEx models;

        public Single3DDisplayViewModel(ModelsEx models)
        {
            this.models = models;
            displayEx = (Single3DDisplayModel) models.Display.ExtendedViewData;

            displayEx.PropertyChanged += DisplayExOnPropertyChanged;
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
                case nameof(DisplayModel.ActiveMipmap):
                    OnPropertyChanged(nameof(FixedAxisSliceMax));
                    break;
            }
        }

        private void DisplayExOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Single3DDisplayModel.FixedAxis):
                    OnPropertyChanged(nameof(SelectedAxis));
                    OnPropertyChanged(nameof(FixedAxisSliceMax));
                    break;
                case nameof(Single3DDisplayModel.FixedAxisSlice):
                    OnPropertyChanged(nameof(FixedAxisSlice));
                    break;
            }
        }

        public List<ComboBoxItem<int>> AxisList { get; } = new List<ComboBoxItem<int>>
        {
            new ComboBoxItem<int>("XY Plane", 2),
            new ComboBoxItem<int>("XZ Plane", 1),
            new ComboBoxItem<int>("YZ Plane", 0),
        };

        public ComboBoxItem<int> SelectedAxis
        {
            get => AxisList[2 - displayEx.FixedAxis];
            set
            {
                if (value == null) return;
                displayEx.FixedAxis = value.Cargo;
            }
        }

        public int FixedAxisSlice
        {
            get => displayEx.FixedAxisSlice;
            set => displayEx.FixedAxisSlice = value;
        }

        public int FixedAxisSliceMax => models.Images.Size.GetMip(models.Display.ActiveMipmap)[displayEx.FixedAxis] - 1;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
