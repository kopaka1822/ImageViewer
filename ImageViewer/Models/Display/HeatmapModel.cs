using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model.Overlay;

namespace ImageViewer.Models.Display
{
    public class HeatmapModel : INotifyPropertyChanged
    {
        private readonly ImageFramework.Model.Models models;
        private HeatmapOverlay overlay = null;

        public HeatmapModel(ImageFramework.Model.Models models)
        {
            this.models = models;
        }
        
        public HeatmapOverlay.Heatmap? Heatmap
        {
            get => overlay?.Data;
            set
            {
                if (value == null && overlay == null) return;

                if(value == null)
                {
                    // delete overlay
                    models.Overlay.Overlays.Remove(overlay);
                    // dispose is managed by the overlay model
                    overlay = null;
                    OnPropertyChanged(nameof(Heatmap));
                    return;
                }

                if(overlay == null)
                {
                    // create a heatmap model in overlays
                    overlay = new HeatmapOverlay(models);
                    models.Overlay.Overlays.Add(overlay);
                }

                overlay.Data = value.Value;
                OnPropertyChanged(nameof(Heatmap));
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
