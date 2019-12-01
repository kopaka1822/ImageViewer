using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.DirectX;
using ImageViewer.Models;
using SharpDX.Mathematics.Interop;
using Color = ImageFramework.Utility.Color;
using Size = System.Drawing.Size;

namespace ImageViewer.Controller
{
    // this controller subscribes to all events that require a repaint and repaints the client
    public class PaintController : IDisposable
    {
        private readonly ModelsEx models;
        private readonly ViewModeController viewMode;
        private SwapChain swapChain = null;
        private bool scheduledRedraw = false;
        private RawColor4 clearColor;
        public PaintController(ModelsEx models)
        {
            this.models = models;
            viewMode = new ViewModeController(models);

            // model events
            models.Display.PropertyChanged += DisplayOnPropertyChanged;
            models.Export.PropertyChanged += ExportOnPropertyChanged;
            models.Window.PropertyChanged += WindowOnPropertyChanged;

            // client mouse events
            models.Window.Window.BorderHost.PreviewMouseMove += (sender, e) => ScheduleRedraw();
            models.Window.Window.BorderHost.PreviewMouseWheel += (sender, e) => ScheduleRedraw();

            foreach (var pipe in models.Pipelines)
            {
                pipe.PropertyChanged += PipeOnPropertyChanged;
            }

            models.Window.Window.Loaded += WindowOnLoaded;

            // clear color
            var bgBrush = (SolidColorBrush)models.Window.Window.FindResource("BackgroundBrush");
            var tmpColor = new Color(bgBrush.Color.ScR, bgBrush.Color.ScG, bgBrush.Color.ScB, 1.0f);
            tmpColor = tmpColor.ToSrgb();
            clearColor.R = tmpColor.Red;
            clearColor.G = tmpColor.Green;
            clearColor.B = tmpColor.Blue;
            clearColor.A = 1.0f;
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
                case nameof(ExportModel.IsExporting):
                    ScheduleRedraw();
                    break;
            }
        }

        private void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            var adapter = new SwapChainAdapter(models.Window.Window.BorderHost);
            models.Window.Window.BorderHost.Child = adapter;
            swapChain = adapter.SwapChain;

            swapChain.Resize(models.Window.ClientSize.Width, models.Window.ClientSize.Height);
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
            try
            {
                var dev = Device.Get();
                dev.ClearRenderTargetView(swapChain.Rtv, clearColor);

                var size = new Size(swapChain.Width, swapChain.Height);
                dev.Rasterizer.SetViewport(0.0f, 0.0f, size.Width, size.Height);
                dev.Rasterizer.SetScissorRectangle(0, 0, size.Width, size.Height);
                dev.OutputMerger.SetRenderTargets(swapChain.Rtv);

                viewMode.Repaint(size);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e.Message, "during repaint");
            }
            finally
            {
                swapChain.EndFrame();
            }
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
                case nameof(DisplayModel.Multiplier):
                case nameof(DisplayModel.DisplayNegative):
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
