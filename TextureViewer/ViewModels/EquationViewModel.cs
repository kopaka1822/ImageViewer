using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class EquationViewModel : INotifyPropertyChanged
    {
        private readonly ImageEquationModel model;

        public EquationViewModel(ImageEquationModel model)
        {
            this.model = model;
            this.model.PropertyChanged += ModelOnPropertyChanged;
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImageEquationModel.Visible):
                    OnPropertyChanged(nameof(IsVisible));
                    return;
                case nameof(ImageEquationModel.UseFilter):
                    OnPropertyChanged(nameof(UseFilter));
                    return;
            }
        }

        public bool IsVisible
        {
            get => model.Visible;
            set => model.Visible = value;
        }

        public bool UseFilter
        {
            get => model.UseFilter;
            set => model.UseFilter = value;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
