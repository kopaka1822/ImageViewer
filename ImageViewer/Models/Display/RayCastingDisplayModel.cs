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
    public class RayCastingDisplayModel : IExtendedDisplayModel
    {
        private readonly ImageFramework.Model.Models models;
        private readonly DisplayModel display;

        public RayCastingDisplayModel(ImageFramework.Model.Models models, DisplayModel display)
        {
            this.models = models;
            this.display = display;
          
        }

        public void Dispose()
        {
            
        }

        

        private bool flatShading = false;

        public bool FlatShading
        {
            get => flatShading;
            set
            {
                if (value == flatShading) return;
                flatShading = value;
                OnPropertyChanged(nameof(FlatShading));
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
