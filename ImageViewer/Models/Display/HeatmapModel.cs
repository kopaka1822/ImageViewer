using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Utility;
using ImageViewer.Models.Display.Overlays;

namespace ImageViewer.Models.Display
{
    // Display model for heatmap
    public class HeatmapModel : INotifyPropertyChanged
    {
        private readonly ImageFramework.Model.Models models;
        private HeatmapOverlay overlay = null;

        public enum ColorStyle
        {
            BlackRed,
            //BlackRedYellowWhite,
            BlackBlueGreenRed
        }

        public HeatmapModel(ImageFramework.Model.Models models)
        {
            this.models = models;
        }

        private Float2 start = Float2.Zero;
        public Float2 Start
        {
            get => start;
            set
            {
                if (value == start) return;
                start = value;
                OnPropertyChanged(nameof(Start));
            }
        }

        private Float2 end = new Float2(0.04f, 0.8f);
        public Float2 End
        {
            get => end;
            set
            {
                if (value == end) return;
                end = value;
                OnPropertyChanged(nameof(End));
            }
        }

        private int border = 2;
        public int Border
        {
            get => border;
            set
            {
                if (value == border) return;
                border = value;
                OnPropertyChanged(nameof(Border));
            }
        }

        private ColorStyle style = ColorStyle.BlackBlueGreenRed;
        public ColorStyle Style
        {
            get => style;
            set
            {
                if (value == style) return;
                style = value;
                OnPropertyChanged(nameof(Style));
            }
        }

        public bool IsEnabled
        {
            get => overlay != null;
            set
            {
                if (value == IsEnabled) return;

                if(value)
                {
                    // create new overlay
                    overlay = new HeatmapOverlay(models, this);
                    models.Overlay.Overlays.Add(overlay);
                }
                else
                {
                    // remove overlay
                    models.Overlay.Overlays.Remove(overlay);
                    // dispose is managed by the overlay model
                    overlay = null;
                }
                OnPropertyChanged(nameof(IsEnabled));
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
