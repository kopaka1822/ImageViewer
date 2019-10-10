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
        private static readonly string[] channelDescriptions = new[]
        {
            // luminance
            @"Luminance is the radiant power weighted by a spectral sensitivity function that is characteristic of vision. 
The magnitude of luminance is proportional to physical power. 
But the spectral composition of luminance is related to the brightness sensitivity of human vision.
Luminance is computed in linear color space with: dot(RGB, (0.2125, 0.7154, 0.0721)).",
            // luma
            @"Luma is brightness computed in sRGB color space which is often used by video codecs.
The ""NTSC"" luma formula is: dot(sRGB, (0.299, 0.587, 0.114)).",
            // lightness
            @"Human vision has a nonlinear perceptual response to brightness: a source having a luminance only 18% of a reference luminance appears about half as bright. 
The perceptual response to luminance Y is called lightness L.
It is computed from the luminance: L = 116 * Y ^ (1/3) - 16."
        };

        public StatisticsViewModel(ModelsEx models)
        {
            this.models = models;
            selectedChannel = AvailableChannels[(int)models.Settings.StatisticsChannel];
            ChannelDescription = channelDescriptions[(int)models.Settings.StatisticsChannel];

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

        public string ChannelDescription { get; private set; } = "";

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
                ChannelDescription = channelDescriptions[(int)value.Cargo];
                OnPropertyChanged(nameof(SelectedChannel));
                OnPropertyChanged(nameof(ChannelDescription));
                models.Settings.StatisticsChannel = value.Cargo;
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
