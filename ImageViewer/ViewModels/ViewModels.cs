using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Models;

namespace ImageViewer.ViewModels
{
    public class ViewModels : IDisposable
    {
        private readonly ModelsEx models;

        public ViewModels(ModelsEx models)
        {
            this.models = models;
        }

        public void Dispose()
        {
            models?.Dispose();
        }
    }
}
