using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.Annotations;
using ImageViewer.Models;

namespace ImageViewer.ViewModels
{
    public class StatisticViewModel : INotifyPropertyChanged
    {
        private readonly int index;
        private readonly ModelsEx models;
        private readonly StatisticModel model;
        private readonly StatisticsViewModel viewModel;

        public StatisticViewModel(int index, ModelsEx models, StatisticsViewModel viewModel)
        {
            this.index = index;
            this.models = models;
            this.viewModel = viewModel;
            this.model = models.Statistics[index];
            

            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
            model.PropertyChanged += ModelOnPropertyChanged;
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(StatisticModel.Stats):
                    if (viewModel.IsVisible)
                    {
                        Update();
                    }
                    break;
            }
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(StatisticsViewModel.IsVisible):
                case nameof(StatisticsViewModel.SelectedChannel):
                    if (viewModel.IsVisible)
                    {
                        Update();
                    }
                    break;
            }
        }

        private void Update()
        {
            OnPropertyChanged(nameof(Average));
            OnPropertyChanged(nameof(Min));
            OnPropertyChanged(nameof(Max));
            OnPropertyChanged(nameof(RootAverage));
        }

        public Visibility Visibility => models.Pipelines[index].IsEnabled ? Visibility.Visible : Visibility.Collapsed;

        public string Average
        {
            get => model.Stats.Avg.Get(viewModel.SelectedChannel.Cargo).ToString(ImageFramework.Model.Models.Culture);
            set { }
        }

        public string RootAverage
        {
            get => Math.Sqrt(model.Stats.Avg.Get(viewModel.SelectedChannel.Cargo)).ToString(ImageFramework.Model.Models.Culture);
            set { }
        }

        public string Min
        {
            get => model.Stats.Min.Get(viewModel.SelectedChannel.Cargo).ToString(ImageFramework.Model.Models.Culture);
            set { }
        }

        public string Max
        {
            get => model.Stats.Max.Get(viewModel.SelectedChannel.Cargo).ToString(ImageFramework.Model.Models.Culture);
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
