using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageViewer.Commands;
using ImageViewer.Models;

namespace ImageViewer.ViewModels
{
    public class ViewModels : IDisposable
    {
        private readonly ModelsEx models;

        public ViewModels(ModelsEx models)
        {
            this.models = models;

            // commands
            ResizeWindow = new ResizeWindowCommand(models);
        }

        public void Dispose()
        {
            models?.Dispose();
        }

        public ICommand ResizeWindow { get; }
    }
}
