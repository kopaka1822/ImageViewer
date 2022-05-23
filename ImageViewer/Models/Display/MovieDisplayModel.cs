using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;

namespace ImageViewer.Models.Display
{
    public class MovieDisplayModel : IExtendedDisplayModel
    {
        private readonly ImageFramework.Model.Models models;
        private readonly DisplayModel display;

        public MovieDisplayModel(ImageFramework.Model.Models models, DisplayModel display)
        {
            this.models = models;
            this.display = display;
        }

        public void Dispose()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler ForceTexelRecompute;

        protected virtual void OnForceTexelRecompute()
        {
            ForceTexelRecompute?.Invoke(this, EventArgs.Empty);
        }
    }
}
