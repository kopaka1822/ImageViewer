using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.ViewModels;

namespace ImageViewer.Commands
{
    class ApplyFiltersCommand : Command
    {
        private readonly FiltersViewModel viewModel;

        public ApplyFiltersCommand(FiltersViewModel viewModel)
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(FiltersViewModel.HasChanges))
                OnCanExecuteChanged();
        }

        public override bool CanExecute()
        {
            return viewModel.HasChanges;
        }

        public override void Execute()
        {
            viewModel.Apply();
        }
    }
}
