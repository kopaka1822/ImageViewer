using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Models;
using TextureViewer.ViewModels;

namespace TextureViewer.Commands
{
    public class ApplyImageFormulasCommand : ICommand
    {
        private readonly EquationsViewModel equations;

        public ApplyImageFormulasCommand(EquationsViewModel equations)
        {
            this.equations = equations;
            equations.PropertyChanged += EquationsOnPropertyChanged;
        }

        private void EquationsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals(nameof(EquationsViewModel.HasChanges)))
                OnCanExecuteChanged();
        }

        public bool CanExecute(object parameter)
        {
            return equations.HasChanges;
        }

        public void Execute(object parameter)
        {
            equations.ApplyFormulas();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
