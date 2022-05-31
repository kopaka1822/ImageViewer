using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using ImageViewer.Models;

namespace ImageViewer.Controller.Overlays
{
    public class HeatmapBoxOverlay : GenericBoxOverlay
    {
        public HeatmapBoxOverlay(ModelsEx models) : base(models)
        {
            models.Heatmap.PropertyChanged += HeatmapOnPropertyChanged;
        }

        private void HeatmapOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (models.Heatmap.Heatmap == null)
            {
                // heatmap was disabled => abort this overlay
                Debug.Assert(ReferenceEquals(this, models.Display.ActiveOverlay));
                if(ReferenceEquals(this, models.Display.ActiveOverlay))
                    models.Display.ActiveOverlay = null;
            }
        }

        protected override void OnFinished(Float2 start, Float2 end)
        {
            if (models.Heatmap.Heatmap == null) return;

            // obtain current setting
            var hm = models.Heatmap.Heatmap.Value;

            // overwrite start and end
            hm.Start = start;
            hm.End = end;
            models.Heatmap.Heatmap = hm;
        }

        public override void Dispose()
        {
            models.Heatmap.PropertyChanged -= HeatmapOnPropertyChanged;
            base.Dispose();
        }
    }
}
