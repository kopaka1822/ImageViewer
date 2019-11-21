using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using ImageFramework.DirectX;
using ImageViewer.Controller.TextureViews;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Controller.TextureViews.Texture2D;
using ImageViewer.Controller.TextureViews.Texture3D;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using SharpDX;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;
using Texture3D = ImageFramework.DirectX.Texture3D;

namespace ImageViewer.Controller
{
    public class ViewModeController : IDisposable
    {
        private readonly ModelsEx models;
        private bool mouseDown = false;
        private bool mouse2Down = false; // middle mouse button
        private Point mousePosition = new Point(0);
        // mouse position for displaying multiple views can be fixed
        private Point? fixedMousePosition = null;
        private readonly Border dxHost;
        private ITextureView currentView = new EmptyView();
        private TextureViewData viewData;
        private bool recomputeScheduled = false;

        public ViewModeController(ModelsEx models)
        {
            this.models = models;
            models.Display.PropertyChanged += DisplayOnPropertyChanged;
            dxHost = models.Window.Window.BorderHost;
            dxHost.MouseWheel += DxHostOnMouseWheel;
            dxHost.MouseDown += DxHostOnMouseDown;
            dxHost.MouseUp += DxHostOnMouseUp;
            dxHost.MouseLeave += DxHostOnMouseLeave;
            dxHost.MouseMove += DxHostOnMouseMove;

            viewData = new TextureViewData(models);
        }

        private void DispatchRecomputeTexelColor()
        {
            if (recomputeScheduled) return;
            
            Dispatcher.CurrentDispatcher.BeginInvoke((Action) RecomputeTexelColor);
            recomputeScheduled = true;
        }

        private void RecomputeTexelColor()
        {
            recomputeScheduled = false;

            models.Display.TexelPosition = currentView.GetTexelPosition(
                ConvertToCanonical(new Vector2(mousePosition.X, mousePosition.Y)));
        }

        /// <summary>
        /// draws the active view
        /// </summary>
        /// <param name="size">actual size in pixel, not dpi independent size</param>
        public void Repaint(Size size)
        {
            var dev = Device.Get();

            var visible = models.GetEnabledPipelines();

            var scissorsPos = new Point(mousePosition.X, mousePosition.Y);

            if (visible.Count < 2) fixedMousePosition = null;
            if(fixedMousePosition != null)
                scissorsPos = new Point(fixedMousePosition.Value.X, fixedMousePosition.Value.Y);

            if (visible.Count == 1)
            {
                // draw single image
                currentView.Draw(models.Pipelines[visible[0]].Image);
            }
            else if (visible.Count == 2)
            {
                // draw two images in split view
               
                scissorsPos.X = Math.Min(size.Width - 1, Math.Max(0, scissorsPos.X));
                scissorsPos.Y = Math.Min(size.Height - 1, Math.Max(0, scissorsPos.Y));

                if(models.Display.Split == DisplayModel.SplitMode.Vertical)
                    dev.Rasterizer.SetScissorRectangle(0, 0, scissorsPos.X, size.Height);
                else 
                    dev.Rasterizer.SetScissorRectangle(0, 0, size.Width, scissorsPos.Y);

                // first image
                currentView.Draw(models.Pipelines[visible[0]].Image);

                if (models.Display.Split == DisplayModel.SplitMode.Vertical)
                    dev.Rasterizer.SetScissorRectangle(scissorsPos.X, 0, size.Width, size.Height);
                else
                    dev.Rasterizer.SetScissorRectangle(0, scissorsPos.Y, size.Width, size.Height);

                // second image
                currentView.Draw(models.Pipelines[visible[1]].Image);
            }
            else if (visible.Count == 3 || visible.Count == 4)
            {
                scissorsPos.X = Math.Min(size.Width - 1, Math.Max(0, scissorsPos.X));
                scissorsPos.Y = Math.Min(size.Height - 1, Math.Max(0, scissorsPos.Y));

                // upper left
                dev.Rasterizer.SetScissorRectangle(0, 0, scissorsPos.X, scissorsPos.Y);
                currentView.Draw(models.Pipelines[visible[0]].Image);

                // upper right
                dev.Rasterizer.SetScissorRectangle(scissorsPos.X, 0, size.Width, scissorsPos.Y);
                currentView.Draw(models.Pipelines[visible[1]].Image);

                // draw third texture (entire bottom if only 3 are visible, lower left if 4 are visible)
                dev.Rasterizer.SetScissorRectangle(0, scissorsPos.Y, visible.Count == 3 ? size.Width : scissorsPos.X, size.Height);
                currentView.Draw(models.Pipelines[visible[2]].Image);

                if(visible.Count == 4)
                {
                    dev.Rasterizer.SetScissorRectangle(scissorsPos.X, scissorsPos.Y, size.Width, size.Height);
                    currentView.Draw(models.Pipelines[visible[3]].Image);
                }
            }
        }

        private void DxHostOnMouseMove(object sender, MouseEventArgs e)
        {
            var newPosition = new Point((int)e.GetPosition(dxHost).X, (int)e.GetPosition(dxHost).Y);

            // don't interrupt when processing
            if (models.Progress.IsProcessing) return;

            if (mouseDown || mouse2Down)
            {
                // drag event
                var diff = new Point(newPosition.X - mousePosition.X, newPosition.Y - mousePosition.Y);
                if (Math.Abs(diff.X) > 0.01 || Math.Abs(diff.Y) > 0.01)
                {
                    if(mouseDown)
                        currentView.OnDrag(new Vector2(diff.X, diff.Y));
                    if(mouse2Down)
                        currentView.OnDrag2(new Vector2(diff.X, diff.Y));
                }
            }

            // set handle to true to keep getting mouse move events
            e.Handled = true;
            mousePosition = newPosition;

            DispatchRecomputeTexelColor();
        }

        private void DxHostOnMouseLeave(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            mouse2Down = false;
        }

        private void DxHostOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                mouseDown = false;
                e.Handled = true;
            } else if (e.ChangedButton == MouseButton.Middle)
            {
                mouse2Down = false;
                e.Handled = true;
            }
                
            mousePosition = new Point((int)e.GetPosition(dxHost).X, (int)e.GetPosition(dxHost).Y);
        }

        private void DxHostOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                mouseDown = true;
                e.Handled = true;
                dxHost.Focus();
            } else if (e.ChangedButton == MouseButton.Middle)
            {
                mouse2Down = true;
                e.Handled = true;
                dxHost.Focus();
            }
                

            mousePosition = new Point((int)e.GetPosition(dxHost).X, (int)e.GetPosition(dxHost).Y);

            // toggle fixed mouse position
            if (e.ChangedButton == MouseButton.Left && e.ClickCount > 1)
            {
                if (fixedMousePosition == null) fixedMousePosition = mousePosition;
                else fixedMousePosition = null;
            }
        }

        private void DxHostOnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // don't interrupt when processing
            if (models.Progress.IsProcessing) return;

            var canMouse = ConvertToCanonical(new Vector2(mousePosition.X, mousePosition.Y));

            // convert to canonical coordinates
            currentView.OnScroll( 
                (float)e.Delta,
                canMouse
            );

            DispatchRecomputeTexelColor();
        }

        private Vector2 ConvertToCanonical(Vector2 windowCoord)
        {
            return new Vector2(
                (float)(windowCoord.X * 2.0 / models.Window.ClientSize.Width - 1.0),
                (float)(windowCoord.Y * 2.0 / models.Window.ClientSize.Height - 1.0)
            );
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.ActiveView):
                    try
                    {
                        currentView.Dispose();
                        currentView = null;
                        switch (models.Display.ActiveView)
                        {
                            case DisplayModel.ViewMode.Single:
                                if(models.Images.ImageType == typeof(Texture3D))
                                    currentView = new Single3DView(models, viewData);
                                else
                                    currentView = new SingleTextureView(models, viewData);
                                break;
                            case DisplayModel.ViewMode.Polar:
                                currentView = new PolarTextureView(models, viewData);
                                break;
                            case DisplayModel.ViewMode.CubeMap:
                                currentView = new CubeTextureView(models, viewData);
                                break;
                            case DisplayModel.ViewMode.CubeCrossView:
                                currentView = new CubeCrossTextureView(models, viewData);
                                break;
                            case DisplayModel.ViewMode.RayCasting:
                                currentView = new RayCastingView(models, viewData);
                                break;
                            default:
                                currentView = new EmptyView();
                                break;
                        }
                    }
                    catch (Exception err)
                    {
                        models.Window.ShowErrorDialog(err.Message);
                        currentView = new EmptyView();
                    }
                    break;
            }
        }

        public void Dispose()
        {
            currentView?.Dispose();
            viewData?.Dispose();
        }
    }
}
