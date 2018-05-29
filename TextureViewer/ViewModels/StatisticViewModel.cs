using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class StatisticViewModel : INotifyPropertyChanged
    {
        private readonly int index;
        private readonly Models.Models models;

        public StatisticViewModel(int index, Models.Models models)
        {
            this.index = index;
            this.models = models;
            this.models.Statistics.StatisticChanged += ModelOnStatisticChanged;
            this.models.Statistics.PropertyChanged += ModelOnPropertyChanged;
            this.models.Equations.Get(index).PropertyChanged += OnEquationPropertyChanged;
        }

        private void OnEquationPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImageEquationModel.Visible):
                    OnPropertyChanged(nameof(Visibility));
                    break;
            }
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(StatisticsModel.Channel):
                case nameof(StatisticsModel.ColorSpace):
                    OnPropertyChanged(nameof(Average));
                    OnPropertyChanged(nameof(Max));
                    OnPropertyChanged(nameof(Min));
                    break;
            }
        }

        private void ModelOnStatisticChanged(object sender, ChangedStatisticEvent e)
        {
            if (e.Index == index)
            {
                OnPropertyChanged(nameof(Average));
                OnPropertyChanged(nameof(Max));
                OnPropertyChanged(nameof(Min));
            }
        }

        public Visibility Visibility => models.Equations.Get(index).Visible ? Visibility.Visible : Visibility.Collapsed;

        public string Average
        {
            get => models.Statistics.Get(index).Avg.Get(models.Statistics.ColorSpace).Get(models.Statistics.Channel).ToString(App.GetCulture());
            set { }
        }

        public string Min
        {
            get => models.Statistics.Get(index).Min.Get(models.Statistics.ColorSpace).Get(models.Statistics.Channel).ToString(App.GetCulture());
            set { }
        }

        public string Max
        {
            get => models.Statistics.Get(index).Max.Get(models.Statistics.ColorSpace).Get(models.Statistics.Channel).ToString(App.GetCulture());
            set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
