using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;

namespace TextureViewer.Models
{
    public class ChangedStatisticEvent : EventArgs
    {
        public ChangedStatisticEvent(int index, StatisticModel model)
        {
            Index = index;
            Model = model;
        }

        public int Index { get; }
        public StatisticModel Model { get; }
    }

    public delegate void ChangedStatisticsHandler(object sender, ChangedStatisticEvent e);

    public class StatisticsModel : INotifyPropertyChanged
    {
        private readonly StatisticModel[] statistics;

        public int NumStatistics => statistics.Length;

        public event ChangedStatisticsHandler StatisticChanged;

        public StatisticsModel()
        {
            statistics = new StatisticModel[App.MaxImageViews];
            for (var i = 0; i < statistics.Length; i++)
            {
                statistics[i] = StatisticModel.ZERO;
            }
        }

        public void UpdateModel(StatisticModel model, int index)
        {
            Debug.Assert(index >= 0 && index <= statistics.Length);
            statistics[index] = model;
            OnStatisticChanged(index);
        }

        public StatisticModel Get(int index)
        {
            Debug.Assert(index >= 0 && index <= statistics.Length);
            return statistics[index];
        }

        public enum ColorSpaceType
        {
            Linear,
            Srgb
        }

        public enum ChannelType
        {
            Red,
            Green,
            Blue,
            Luminance
        }

        private ColorSpaceType colorSpace = ColorSpaceType.Linear;
        public ColorSpaceType ColorSpace
        {
            get => colorSpace;
            set
            {
                if (value == colorSpace) return;
                colorSpace = value;
                OnPropertyChanged(nameof(ColorSpace));
            }
        }

        private ChannelType channel = ChannelType.Luminance;
        public ChannelType Channel
        {
            get => channel;
            set
            {
                if (value == channel) return;
                channel = value;
                OnPropertyChanged(nameof(Channel));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnStatisticChanged(int index)
        {
            StatisticChanged?.Invoke(this, new ChangedStatisticEvent(index, statistics[index]));
        }
    }
}
