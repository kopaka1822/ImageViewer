using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ImageFramework.Model;
using ImageViewer.Models;

namespace ImageViewer.Controller
{
    public class ComputeImageController
    {
        private readonly ModelsEx models;
        private bool isRunning = false;

        public ComputeImageController(ModelsEx models)
        {
            this.models = models;
            foreach (var pipe in models.Pipelines)
            {
                pipe.PropertyChanged += PipeOnPropertyChanged;
            }
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var pipe = (ImagePipeline) sender;
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.Image):
                    // if image changed to null => recompute
                    if(pipe.Image == null)
                        SheduleRecompute();
                    break;
                case nameof(ImagePipeline.IsValid):
                    // if image changed from invalid to valid => recompute
                    if(pipe.IsValid)
                        SheduleRecompute();
                    break;
            }
        }

        private void SheduleRecompute()
        {
            Dispatcher.CurrentDispatcher.BeginInvoke((Action)(Execute));
        }

        public void Execute()
        {
            ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            isRunning = true;
            try
            {
                var cts = new CancellationTokenSource();
                await models.ApplyAsync(cts.Token);
            }
            finally
            {
                isRunning = false;
            }
        }
    }
}
