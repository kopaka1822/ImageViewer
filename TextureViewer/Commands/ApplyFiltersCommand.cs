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
    public class ApplyFiltersCommand : ICommand
    {
        private readonly FiltersViewModel viewModel;


        public ApplyFiltersCommand(FiltersViewModel viewModel)
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
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
            viewModel.Apply();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
