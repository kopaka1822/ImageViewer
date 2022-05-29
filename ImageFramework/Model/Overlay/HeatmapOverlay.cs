using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;

namespace ImageFramework.Model.Overlay
{
    public class HeatmapOverlay : OverlayBase
    {
        public override void Render(LayerMipmapSlice lm, Size3 size)
        {
            Debug.Assert(HasWork);
        }

        public override void Dispose()
        {
            
        }

        private bool isEnabled = false;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (value == isEnabled) return;
                isEnabled = value;
                OnHasChanged();
            }
        }

        public override bool HasWork => IsEnabled;
    }
}
