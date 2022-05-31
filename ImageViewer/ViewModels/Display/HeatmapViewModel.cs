using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Controller.Overlays;
using ImageViewer.Models;
using ImageViewer.Models.Display;

namespace ImageViewer.ViewModels.Display
{
    public class HeatmapViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;
        private HeatmapOverlay.Heatmap lastHeatmap;

        public HeatmapViewModel(ModelsEx models)
        {
            this.models = models;
            models.Heatmap.PropertyChanged += HeatmapOnPropertyChanged;

            SetPositionCommand = new ActionCommand(SetPosition);

            // init last heatmap
            lastHeatmap = new HeatmapOverlay.Heatmap
            {
                Border = 2,
                Start = Float2.Zero,
                End = new Float2(0.01f, 0.4f),
                Style = HeatmapOverlay.Style.BlackRed
            };
        }

        private void HeatmapOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(HeatmapModel.Heatmap):
                    OnPropertyChanged(nameof(IsEnabled));
                    // save last state
                    if (models.Heatmap.Heatmap.HasValue)
                        lastHeatmap = models.Heatmap.Heatmap.Value;
                    break;
            }
        }

        public void SetPosition()
        {
            Debug.Assert(IsEnabled);

            var overlay = new HeatmapBoxOverlay(models);
            models.Display.ActiveOverlay = overlay;
        }

        public ICommand SetPositionCommand { get; }

        public ICommand ConfigureCommand { get; }

        public bool IsEnabled
        {
            get => models.Heatmap.Heatmap.HasValue;
            set
            {
                if (value == IsEnabled) return;

                if (value)
                {
                    // set to state before it was disabled
                    models.Heatmap.Heatmap = lastHeatmap;
                }
                else
                {
                    models.Heatmap.Heatmap = null;
                }
                
                // OnPropertyChanged(nameof(IsEnabled));
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
