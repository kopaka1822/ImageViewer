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
    public class StatisticViewModel : INotifyPropertyChanged
    {
        private readonly int index;
        private readonly StatisticsModel model;

        public StatisticViewModel(int index, StatisticsModel model)
        {
            this.index = index;
            this.model = model;
            this.model.StatisticChanged += ModelOnStatisticChanged;
            this.model.PropertyChanged += ModelOnPropertyChanged;
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

        public string Average
        {
            get => model.Get(index).Avg.Get(model.ColorSpace).Get(model.Channel).ToString(App.GetCulture());
            set { }
        }

        public string Min
        {
            get => model.Get(index).Min.Get(model.ColorSpace).Get(model.Channel).ToString(App.GetCulture());
            set { }
        }

        public string Max
        {
            get => model.Get(index).Max.Get(model.ColorSpace).Get(model.Channel).ToString(App.GetCulture());
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
