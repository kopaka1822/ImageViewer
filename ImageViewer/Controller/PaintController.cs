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
using ImageFramework.DirectX.Query;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Model.Overlay;
using ImageViewer.DirectX;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using SharpDX;
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
        private bool scheduledRedraw = false;
        private RawColor4 clearColor;
        private readonly AdvancedGpuTimer gpuTimer;
        public PaintController(ModelsEx models)
        {
            this.models = models;
            this.gpuTimer = new AdvancedGpuTimer();
            viewMode = new ViewModeController(models);

            // model events
            models.Display.PropertyChanged += DisplayOnPropertyChanged;
            models.Window.PropertyChanged += WindowOnPropertyChanged;
            models.Overlay.PropertyChanged += OverlayOnPropertyChanged;

            // client mouse events
            models.Window.Window.BorderHost.PreviewMouseMove += (sender, e) => ScheduleRedraw();
            models.Window.Window.BorderHost.PreviewMouseWheel += (sender, e) => ScheduleRedraw();

            foreach (var pipe in models.Pipelines)
            {
                pipe.PropertyChanged += PipeOnPropertyChanged;
            }

            // clear color
            var col = models.Window.ThemeColor;
            clearColor.R = col.Red;
            clearColor.G = col.Green;
            clearColor.B = col.Blue;
            clearColor.A = 1.0f;
        }

        private void OverlayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OverlayModel.Overlay):
                    ScheduleRedraw();
                    break;
            }
            
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
            var swapChain = models.Window.SwapChain;
            if (swapChain == null || swapChain.IsDisposed) return;

            var timerStarted = false;
            swapChain.BeginFrame();
            try
            {
                var dev = Device.Get();
                // recompute overlay before binding rendertargets
                models.Overlay.Recompute();

                dev.ClearRenderTargetView(swapChain.Rtv, clearColor);

                var size = new Size(swapChain.Width, swapChain.Height);
                dev.Rasterizer.SetViewport(0.0f, 0.0f, size.Width, size.Height);
                dev.Rasterizer.SetScissorRectangle(0, 0, size.Width, size.Height);
                dev.OutputMerger.SetRenderTargets(swapChain.Rtv);

                gpuTimer.Start();
                timerStarted = true;

                viewMode.Repaint(size);

            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e, "during repaint");
            }
            finally
            {
                if (timerStarted)
                {
                    gpuTimer.Stop();
                }

                try
                {

                    swapChain.EndFrame();
                }
                catch (SharpDXException e)
                {
                    models.Window.ShowErrorDialog(e);
                    models.Window.Window.Close();
                }
            }

            models.Display.FrameTime = gpuTimer.Get();
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
                case nameof(DisplayModel.IsExporting):
                    ScheduleRedraw();
                    break;
                case nameof(DisplayModel.ExtendedViewData):
                    if (models.Display.ExtendedViewData != null)
                    {
                        models.Display.ExtendedViewData.PropertyChanged += (s, ev) => ScheduleRedraw();
                    }
                    break;
            }
        }


        public void Dispose()
        {
            viewMode?.Dispose();
            gpuTimer?.Dispose();
        }
    }
}
