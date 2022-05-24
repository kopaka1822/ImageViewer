using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public bool OnKeyDown(Key key)
        {
            // TODO implement
            switch (key)
            {
                case Key.OemComma:
                    //viewModel.PreviousFrame();
                    break;
                case Key.OemPeriod:
                    //viewModel.NextFrame();
                    break;
                case Key.Space:
                    //viewModel.PlayPause();
                    break;
                default:
                    return false;
            }
            return true;
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
