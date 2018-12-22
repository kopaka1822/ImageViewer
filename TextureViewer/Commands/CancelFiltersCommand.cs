using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.ViewModels;

namespace TextureViewer.Commands
{
    public class CancelFiltersCommand : ICommand
    {
        private readonly FiltersViewModel viewModel;

        public CancelFiltersCommand(FiltersViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(FiltersViewModel.HasChanges))
            {
                OnCanExecuteChanged();
            }
        }

        public bool CanExecute(object parameter)
        {
            return viewModel.HasChanges;
        }

        public void Execute(object parameter)
        {
            viewModel.Cancel();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
