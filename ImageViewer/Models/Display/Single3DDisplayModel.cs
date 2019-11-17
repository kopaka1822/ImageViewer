using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Utility;

namespace ImageViewer.Models.Display
{
    public class Single3DDisplayModel : IExtendedDisplayModel
    {
        private readonly ImageFramework.Model.Models models;
        private readonly DisplayModel display;

        public Single3DDisplayModel(ImageFramework.Model.Models models, DisplayModel display)
        {
            this.models = models;
            this.display = display;
            this.display.PropertyChanged += DisplayOnPropertyChanged;
        }

        public void Dispose()
        {
            this.display.PropertyChanged -= DisplayOnPropertyChanged;
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.ActiveMipmap):
                    FixedAxisSlice = FixedAxisSlice; // refresh slice (might get clamped)
                    break;
            }
        }

        private int fixedAxis = 2; // z-axis is fixed

        public int FixedAxis
        {
            get => fixedAxis;
            set
            {
                if (value == fixedAxis) return;
                fixedAxis = Utility.Clamp(value, 0, 2);
                OnPropertyChanged(nameof(FixedAxis));
            }
        }

        public int FreeAxis1 => FixedAxis == 0 ? 1 : 0;
        public int FreeAxis2 => FixedAxis == 2 ? 1 : 2;

        private int fixedAxisSlice = 0;

        public int FixedAxisSlice
        {
            get => fixedAxisSlice;
            set
            {
                var dim = models.Images.Size.GetMip(display.ActiveMipmap);
                var clamped = Utility.Clamp(value, 0, dim[fixedAxis] - 1);

                if (fixedAxisSlice == clamped && value == fixedAxisSlice) return;

                fixedAxisSlice = clamped;
                OnPropertyChanged(nameof(FixedAxisSlice));
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
