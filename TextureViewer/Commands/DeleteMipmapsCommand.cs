using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Models;

namespace TextureViewer.Commands
{
    class DeleteMipmapsCommand : ICommand
    {
        private Models.Models models;

        public DeleteMipmapsCommand(Models.Models models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumMipmaps):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public void Execute(object parameter)
        {
            Debug.Assert(CanExecute(null));

            models.Images.DeleteMipmaps();
        }

        public bool CanExecute(object parameter)
        {
            return models.Images.NumMipmaps > 1;
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;
    }
}
