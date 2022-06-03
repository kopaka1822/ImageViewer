using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ImageFramework.Model;
using ImageFramework.Model.Progress;
using ImageViewer.Models;

namespace ImageViewer.Controller
{
    public class ComputeImageController
    {
        private readonly ModelsEx models;
        private bool scheduled = false;

        public ComputeImageController(ModelsEx models)
        {
            this.models = models;
            foreach (var pipe in models.Pipelines)
            {
                pipe.PropertyChanged += PipeOnPropertyChanged;
            }
            models.PipelineUpdate += OnPipelineUpdate;
        }

        private void OnPipelineUpdate(object sender, bool success)
        {
            // task may be rescheduled (update finished)
            scheduled = false;
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var pipe = (ImagePipeline) sender;
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.Image):
                    // if image changed to null => recompute
                    if(pipe.Image == null)
                        ScheduleRecompute();
                    break;
                case nameof(ImagePipeline.IsValid):
                    // if image changed from invalid to valid => recompute
                    if(pipe.IsValid)
                        ScheduleRecompute();
                    break;
                case nameof(ImagePipeline.IsEnabled):
                    // change from not enabled to enabled
                    if(pipe.IsEnabled)
                        ScheduleRecompute();
                    break;
                
            }
        }

        public void ScheduleRecompute()
        {
            if (scheduled) return;
            scheduled = true;
            Dispatcher.CurrentDispatcher.BeginInvoke((Action)(Execute));
        }

        public async void Execute()
        {
            await models.ApplyAsync();
            if(!String.IsNullOrEmpty(models.Progress.LastError))
                models.Window.ShowErrorDialog(models.Progress.LastError);
        }
    }
}
