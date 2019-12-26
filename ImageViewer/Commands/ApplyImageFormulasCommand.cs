using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels;

namespace ImageViewer.Commands
{
    public class ApplyImageFormulasCommand : Command
    {
        private readonly EquationsViewModel viewModel;

        public ApplyImageFormulasCommand(EquationsViewModel viewModel)
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(EquationsViewModel.HasChanges)))
                OnCanExecuteChanged();
        }

        public override bool CanExecute()
        {
            return viewModel.HasChanges;
        }

        public override void Execute()
        {
            viewModel.ApplyFormulas();
        }
    }
}
