using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.DirectX;
using ImageViewer.Models;

namespace ImageViewer.Controller
{
    // this controller subscribes to all events that require a repaint and repaints the client
    public class PaintController : IDisposable
    {
        private readonly ModelsEx models;
        private readonly ViewModeController viewMode;
        private SwapChain swapChain = null;
        private bool scheduledRedraw = false;

        public PaintController(ModelsEx models)
        {
            this.models = models;
            viewMode = new ViewModeController(models);

            // model events
            models.Display.PropertyChanged += DisplayOnPropertyChanged;
            models.Export.PropertyChanged += ExportOnPropertyChanged;
            models.Window.PropertyChanged += WindowOnPropertyChanged;

            // client mouse events
            models.Window.Window.BorderHost.MouseMove += (sender, e) => ScheduleRedraw();
            models.Window.Window.BorderHost.MouseWheel += (sender, e) => ScheduleRedraw();

            foreach (var pipe in models.Pipelines)
            {
                pipe.PropertyChanged += PipeOnPropertyChanged;
            }

            models.Window.Window.Loaded += WindowOnLoaded;
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.IsEnabled):
                case nameof(ImagePipeline.Image):
                    ScheduleRedraw();
                    break;
            }
        }

        private void WindowOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WindowModel.ClientSize):
                    ScheduleRedraw();
                    break;
            }
        }

        private void ExportOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ExportModel.CropStartX):
                case nameof(ExportModel.CropStartY):
                case nameof(ExportModel.CropEndX):
                case nameof(ExportModel.CropEndY):
                case nameof(ExportModel.UseCropping):
                case nameof(ExportModel.Mipmap):
                case nameof(ExportModel.Layer):
                    ScheduleRedraw();
                    break;
            }
        }

        private void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            var adapter = new SwapChainAdapter(models.Window.Window.BorderHost);
            models.Window.Window.BorderHost.Child = adapter;
            swapChain = adapter.SwapChain;
        }

        /// <summary>
        /// the frame will be redrawn as soon as possible
        /// </summary>
        private void ScheduleRedraw()
        {
            if (!scheduledRedraw)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)OnRepaint);
                scheduledRedraw = true;
            }
        }

        /// <summary>
        /// this repaints the client area
        /// </summary>
        private void OnRepaint()
        {
            scheduledRedraw = false;
            if (swapChain == null) return;

            swapChain.BeginFrame();

            // paint stuff
            viewMode.Repaint();

            swapChain.EndFrame();
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.ActiveView):
                case nameof(DisplayModel.ActiveLayer):
                case nameof(DisplayModel.ActiveMipmap):
                case nameof(DisplayModel.Zoom):
                case nameof(DisplayModel.Aperture):
                case nameof(DisplayModel.LinearInterpolation):
                case nameof(DisplayModel.ShowCropRectangle):
                    ScheduleRedraw();
                    break;
            }
        }


        public void Dispose()
        {
            viewMode?.Dispose();
            swapChain?.Dispose();
        }
    }
}
