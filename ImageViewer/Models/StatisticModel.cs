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
        private readonly DisplayModel display;
        private readonly ImagePipeline pipe;

        public StatisticModel(ImageFramework.Model.Models models, DisplayModel display, int index)
        {
            this.models = models;
            this.display = display;
            pipe = this.models.Pipelines[index];
            pipe.PropertyChanged += PipeOnPropertyChanged;
            display.PropertyChanged += DisplayOnPropertyChanged;
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.ActiveLayer):
                    if (display.ActiveView == DisplayModel.ViewMode.CubeCrossView ||
                        display.ActiveView == DisplayModel.ViewMode.CubeMap)
                        return; // does not need to be recomputed

                    UpdateStatistics();
                    break;
                case nameof(DisplayModel.ActiveMipmap):
                case nameof(DisplayModel.ActiveView):
                    UpdateStatistics();
                    break;
            }
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.Image):
                    UpdateStatistics();
                    break;
            }
        }

        private void UpdateStatistics()
        {
            if (pipe.Image != null)
            {
                if (display.ActiveView == DisplayModel.ViewMode.CubeCrossView ||
                    display.ActiveView == DisplayModel.ViewMode.CubeMap)
                {
                    // compute for all layers
                    Stats = StatisticsModel.Zero;
                    for (int i = 0; i < models.Images.NumLayers; ++i)
                    {
                        Stats.Plus(models.GetStatistics(pipe.Image, i, display.ActiveMipmap));
                    }

                    Stats.Divide((float) models.Images.NumLayers);
                }
                else // compute for single layer
                {
                    Stats = models.GetStatistics(pipe.Image, display.ActiveLayer, display.ActiveMipmap);
                }
                
                OnPropertyChanged(nameof(Stats));
            }
            else
            {
                Stats = StatisticsModel.Zero;
                OnPropertyChanged(nameof(Stats));
            }
            
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
