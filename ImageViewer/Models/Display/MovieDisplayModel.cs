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

        // this is overwritten by the view model to connect the keys
        public Func<Key, bool> OnKeyFunc { get; set; } 

        public bool OnKeyDown(Key key)
        {
            Debug.Assert(OnKeyFunc != null);
            return OnKeyFunc(key);
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
