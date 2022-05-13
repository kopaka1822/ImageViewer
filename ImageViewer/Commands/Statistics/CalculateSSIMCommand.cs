using ImageViewer.Commands.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.ViewModels.Statistics;

namespace ImageViewer.Commands.Statistics
{
    public class CalculateSSIMCommand : SimpleCommand
    {
        private SSIMsViewModel viewModel;

        public CalculateSSIMCommand(SSIMsViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public override void Execute()
        {
            foreach (var ssimViewModel in viewModel.Items)
            {
                ssimViewModel.RecalculateSSIM();
            }
        }
    }
}
