using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageFramework.Model.Shader;

namespace ImageViewer.Models
{
    public class StatisticModel : INotifyPropertyChanged
    {
        private readonly ImageFramework.Model.Models models;
        private readonly ImagePipeline pipe;

        public StatisticModel(ImageFramework.Model.Models models, int index)
        {
            this.models = models;
            pipe = this.models.Pipelines[index];
            pipe.PropertyChanged += PipeOnPropertyChanged;
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.Image):
                    if(pipe.Image != null)
                        UpdateStatistics(); // image was refreshed
                    else
                    {
                        Stats = StatisticsModel.Zero;
                        OnPropertyChanged(nameof(Stats));
                    }
                    break;
            }
        }

        private void UpdateStatistics()
        {
            // TODO calculate statistics for all layers and one mipmap
            Stats = models.GetStatistics(pipe.Image);
            OnPropertyChanged(nameof(Stats));
        }

        public StatisticsModel Stats { get; private set; } = StatisticsModel.Zero;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
