using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageFramework.Model.Shader;
using ImageViewer.Models;
using ImageViewer.Views;

namespace ImageViewer.ViewModels
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;
        private readonly StatisticViewModel[] viewModels;

        public StatisticsViewModel(ModelsEx models)
        {
            this.models = models;
            selectedChannel = AvailableChannels[(int)models.Settings.StatisticsChannel];

            viewModels = new StatisticViewModel[models.NumPipelines];
            for (int i = 0; i < viewModels.Length; ++i)
            {
                viewModels[i] = new StatisticViewModel(i, models, this);
            }

            models.Window.Window.TabControl.SelectionChanged += TabControlOnSelectionChanged;
        }

        private void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            IsVisible = ReferenceEquals(models.Window.Window.TabControl.SelectedItem, models.Window.Window.StatisticsTabItem);
        }

        public StatisticViewModel Equation1 => viewModels[0];
        public StatisticViewModel Equation2 => viewModels[1];
        public StatisticViewModel Equation3 => viewModels[2];
        public StatisticViewModel Equation4 => viewModels[3];

        private bool isVisible = false;
        public bool IsVisible
        {
            get => isVisible;
            private set
            {
                if (isVisible == value) return;
                isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public List<ComboBoxItem<DefaultStatistics.Values>> AvailableChannels { get; } = new List<ComboBoxItem<DefaultStatistics.Values>>
        {
            new ComboBoxItem<DefaultStatistics.Values>("Luminance", DefaultStatistics.Values.Luminance),
            new ComboBoxItem<DefaultStatistics.Values>("Luma", DefaultStatistics.Values.Luma),
            new ComboBoxItem<DefaultStatistics.Values>("Lightness", DefaultStatistics.Values.Lightness),
        };

        private ComboBoxItem<DefaultStatistics.Values> selectedChannel;
        public ComboBoxItem<DefaultStatistics.Values> SelectedChannel
        {
            get => selectedChannel;
            set
            {
                if (ReferenceEquals(value, selectedChannel)) return;
                selectedChannel = value;
                OnPropertyChanged(nameof(SelectedChannel));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
