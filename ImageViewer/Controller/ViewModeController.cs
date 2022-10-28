using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using ImageFramework.DirectX;
using ImageFramework.Model;
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
using Size = System.Drawing.Size;
using Texture3D = ImageFramework.DirectX.Texture3D;

namespace ImageViewer.Controller
{
    public class ViewModeController : IDisposable
    {
        private readonly ModelsEx models;
        private bool mouseDown = false;
        private bool mouse2Down = false; // middle mouse button
        // mouse position in DPI coordinates (based on models.Window.ClientSize)
        private Vector2 mousePosition = new Vector2(0.0f, 0.0f);
        // mouse position for displaying multiple views can be fixed
        private Vector2? fixedMousePosition = null;
        private readonly Border dxHost;
        private ITextureView currentView = new EmptyView();
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

            for(int i = 0; i < models.NumPipelines; ++i)
            {
                var i1 = i;
                models.Pipelines[i].PropertyChanged += (o, e) => OnPipelineChanged(models.Pipelines[i1], e, i1);
            }
        }

        private void OnPipelineChanged(ImagePipeline pipe, PropertyChangedEventArgs e, int id)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.Image):
                    currentView?.UpdateImage(id, pipe.Image);
                    break;
            }
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
                ConvertToCanonical(mousePosition));
        }

        /// <summary>
        /// draws the active view
        /// </summary>
        /// <param name="size">actual size in pixel, not dpi independent size</param>
        public void Repaint(Size size)
        {
            var dev = Device.Get();

            if (currentView == null) return; // some error occured

            var visible = models.GetEnabledPipelines();

            // mouse position needs to be converted from dpi aware pixels to actual screen pixels
            var scissorsPos = ConvertFromDpiToPixels(mousePosition, size);

            if (visible.Count < 2) fixedMousePosition = null;
            if (fixedMousePosition.HasValue)
                scissorsPos = ConvertFromDpiToPixels(fixedMousePosition.Value, size);

            if (visible.Count == 1)
            {
                // draw single image
                currentView.Draw(visible[0],models.Pipelines[visible[0]].Image);
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
                currentView.Draw(visible[0], models.Pipelines[visible[0]].Image);

                if (models.Display.Split == DisplayModel.SplitMode.Vertical)
                    dev.Rasterizer.SetScissorRectangle(scissorsPos.X, 0, size.Width, size.Height);
                else
                    dev.Rasterizer.SetScissorRectangle(0, scissorsPos.Y, size.Width, size.Height);

                // second image
                currentView.Draw(visible[1], models.Pipelines[visible[1]].Image);
            }
            else if (visible.Count == 3 || visible.Count == 4)
            {
                scissorsPos.X = Math.Min(size.Width - 1, Math.Max(0, scissorsPos.X));
                scissorsPos.Y = Math.Min(size.Height - 1, Math.Max(0, scissorsPos.Y));

                // upper left
                dev.Rasterizer.SetScissorRectangle(0, 0, scissorsPos.X, scissorsPos.Y);
                currentView.Draw(visible[0], models.Pipelines[visible[0]].Image);

                // upper right
                dev.Rasterizer.SetScissorRectangle(scissorsPos.X, 0, size.Width, scissorsPos.Y);
                currentView.Draw(visible[1], models.Pipelines[visible[1]].Image);

                // draw third texture (entire bottom if only 3 are visible, lower left if 4 are visible)
                dev.Rasterizer.SetScissorRectangle(0, scissorsPos.Y, visible.Count == 3 ? size.Width : scissorsPos.X, size.Height);
                currentView.Draw(visible[2], models.Pipelines[visible[2]].Image);

                if(visible.Count == 4)
                {
                    dev.Rasterizer.SetScissorRectangle(scissorsPos.X, scissorsPos.Y, size.Width, size.Height);
                    currentView.Draw(visible[3], models.Pipelines[visible[3]].Image);
                }
            }
        }

        private void DxHostOnMouseMove(object sender, MouseEventArgs e)
        {
            var newPosition = new Vector2((float)e.GetPosition(dxHost).X, (float)e.GetPosition(dxHost).Y);

            if (mouseDown || mouse2Down)
            {
                // drag event
                var diff = newPosition - mousePosition;
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
                
            mousePosition = new Vector2((float)e.GetPosition(dxHost).X, (float)e.GetPosition(dxHost).Y);

            if(models.Display.TexelPosition.HasValue)
                models.Display.ActiveOverlay?.MouseClick(e.ChangedButton, false, models.Display.TexelPosition.Value);
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
                

            mousePosition = new Vector2((float)e.GetPosition(dxHost).X, (float)e.GetPosition(dxHost).Y);

            // toggle fixed mouse position
            if (e.ChangedButton == MouseButton.Left && e.ClickCount > 1)
            {
                if (fixedMousePosition == null) fixedMousePosition = mousePosition;
                else fixedMousePosition = null;
            }

            if (models.Display.TexelPosition.HasValue)
                models.Display.ActiveOverlay?.MouseClick(e.ChangedButton, true, models.Display.TexelPosition.Value);
        }

        private void DxHostOnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var canMouse = ConvertToCanonical(new Vector2(mousePosition.X, mousePosition.Y));

            // convert to canonical coordinates
            currentView.OnScroll( 
                (float)e.Delta,
                canMouse
            );

            DispatchRecomputeTexelColor();
        }

        // converts from [0, models.Window.ClientSize] to [-1, 1]
        private Vector2 ConvertToCanonical(Vector2 windowCoord)
        {
            return new Vector2(
                (float)(windowCoord.X * 2.0 / models.Window.ClientSize.Width - 1.0),
                (float)(windowCoord.Y * 2.0 / models.Window.ClientSize.Height - 1.0)
            );
        }

        // converts from [0, models.Window.ClientSize] to [0, pixelSize]
        private Point ConvertFromDpiToPixels(Vector2 dpiCoord, Size pixelSize)
        {
            return new Point(
                (int)(dpiCoord.X * pixelSize.Width / models.Window.ClientSize.Width),
                (int)(dpiCoord.Y * pixelSize.Height / models.Window.ClientSize.Height) 
            );
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.ActiveMipmap):
                    // recompute texel position
                    DispatchRecomputeTexelColor();
                    break;
                case nameof(DisplayModel.ExtendedViewData):
                    if (models.Display.ExtendedViewData == null) return;
                    models.Display.ExtendedViewData.ForceTexelRecompute += (o, ev) => DispatchRecomputeTexelColor();
                    break;
                case nameof(DisplayModel.ExtendedStatusbarData):
                    if (models.Display.ExtendedStatusbarData == null) return;
                    models.Display.ExtendedStatusbarData.ForceTexelRecompute += (o, ev) => DispatchRecomputeTexelColor();
                    break;
                case nameof(DisplayModel.ActiveView):
                    try
                    {
                        currentView.Dispose();
                        currentView = null;
                        switch (models.Display.ActiveView)
                        {
                            case DisplayModel.ViewMode.Single:
                                if(models.Images.ImageType == typeof(Texture3D))
                                    currentView = new Single3DView(models);
                                else
                                    currentView = new SingleTextureView(models);
                                break;
                            case DisplayModel.ViewMode.Polar:
                                currentView = new PolarTextureView(models);
                                break;
                            case DisplayModel.ViewMode.CubeMap:
                                currentView = new CubeTextureView(models);
                                break;
                            case DisplayModel.ViewMode.CubeCrossView:
                                currentView = new CubeCrossTextureView(models);
                                break;
                            case DisplayModel.ViewMode.Volume:
                                currentView = new VolumeView(models);
                                break;
                            case DisplayModel.ViewMode.ShearWarp:
                                Debug.Assert(false);
                                //currentView = new ShearWarpView(models);
                                break;
                            default:
                                currentView = new EmptyView();
                                break;
                        }

                        for(int i = 0; i < models.NumPipelines; ++i)
                        {
                            if(models.Pipelines[i].Image != null)
                                currentView.UpdateImage(i, models.Pipelines[i].Image);
                        }
                    }
                    catch (Exception err)
                    {
                        models.Window.ShowErrorDialog(err);
                        currentView = new EmptyView();
                    }
                    break;
            }
        }

        public void Dispose()
        {
            currentView?.Dispose();
        }
    }
}
