using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TextureViewer.Annotations;
using TextureViewer.Models;
using TextureViewer.Views;

namespace TextureViewer.ViewModels
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private readonly StatisticViewModel[] viewModels;

        public StatisticsViewModel(Models.Models models)
        {
            this.models = models;
            selectedColorSpace = AvailableColorSpaces[0];
            selectedChannel = AvailableChannels[0];
            this.models.Statistics.PropertyChanged += StatisticsOnPropertyChanged;

            viewModels = new StatisticViewModel[models.Statistics.NumStatistics];
            for (int i = 0; i < viewModels.Length; ++i)
            {
                viewModels[i] = new StatisticViewModel(i, models, this);
            }

            models.App.Window.TabControl.SelectionChanged += TabControlOnSelectionChanged;
        }

        private void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            IsVisible = ReferenceEquals(models.App.Window.TabControl.SelectedItem, models.App.Window.StatisticsTabItem);
        }

        private void StatisticsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(StatisticsModel.Channel):
                    SelectedChannel = AvailableChannels.Find(box => box.Cargo == models.Statistics.Channel);
                    break;
                case nameof(StatisticsModel.ColorSpace):
                    SelectedColorSpace = AvailableColorSpaces.Find(box => box.Cargo == models.Statistics.ColorSpace);
                    break;
            }
        }

        public StatisticViewModel Equation1 => viewModels[0];
        public StatisticViewModel Equation2 => viewModels[1];

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

        public List<ComboBoxItem<StatisticsModel.ColorSpaceType>> AvailableColorSpaces { get; } = new List<ComboBoxItem<StatisticsModel.ColorSpaceType>>
        {
            new ComboBoxItem<StatisticsModel.ColorSpaceType>("Linear", StatisticsModel.ColorSpaceType.Linear),
            new ComboBoxItem<StatisticsModel.ColorSpaceType>("Srgb", StatisticsModel.ColorSpaceType.Srgb)
        };

        private ComboBoxItem<StatisticsModel.ColorSpaceType> selectedColorSpace;
        public ComboBoxItem<StatisticsModel.ColorSpaceType>  SelectedColorSpace
        {
            get => selectedColorSpace;
            set
            {
                if(ReferenceEquals(selectedColorSpace, value)) return;
                selectedColorSpace = value;
                models.Statistics.ColorSpace = value.Cargo;
                OnPropertyChanged(nameof(SelectedColorSpace));
            }
        }

        public List<ComboBoxItem<StatisticsModel.ChannelType>> AvailableChannels { get; } = new List<ComboBoxItem<StatisticsModel.ChannelType>>
        {
            new ComboBoxItem<StatisticsModel.ChannelType>("Luminance", StatisticsModel.ChannelType.Luminance),
            new ComboBoxItem<StatisticsModel.ChannelType>("Red", StatisticsModel.ChannelType.Red),
            new ComboBoxItem<StatisticsModel.ChannelType>("Green", StatisticsModel.ChannelType.Green),
            new ComboBoxItem<StatisticsModel.ChannelType>("Blue", StatisticsModel.ChannelType.Blue),
        };

        private ComboBoxItem<StatisticsModel.ChannelType> selectedChannel;
        public ComboBoxItem<StatisticsModel.ChannelType> SelectedChannel
        {
            get => selectedChannel;
            set
            {
                if (ReferenceEquals(value, selectedChannel)) return;
                selectedChannel = value;
                models.Statistics.Channel = value.Cargo;
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
