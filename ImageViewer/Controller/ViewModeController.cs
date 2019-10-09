using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ImageViewer.Controller.TextureViews;
using ImageViewer.Models;
using SharpDX;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller
{
    public class ViewModeController : IDisposable
    {
        private readonly ModelsEx models;
        private bool mouseDown = false;
        private Point mousePosition = new Point(0);
        private readonly Border dxHost;
        private ITextureView currentView = new EmptyView();
        private TextureViewData viewData;

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

            viewData = new TextureViewData();
        }

        public void Repaint()
        {
            var dev = Device.Get();
            var size = models.Window.ClientSize;

            var visible = models.GetEnabledPipelines();
            if (visible.Count == 1)
            {
                // draw single image
                currentView.Draw(models.Pipelines[visible[0]].Image);
            }
            else if (visible.Count == 2)
            {
                // draw two images in split view
                var scissorsPos = new Point(mousePosition.X, mousePosition.Y);
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
                var scissorsPos = new Point(mousePosition.X, mousePosition.Y);
                scissorsPos.X = Math.Min(size.Width - 1, Math.Max(0, scissorsPos.X));
                scissorsPos.Y = Math.Min(size.Height - 1, Math.Max(0, scissorsPos.Y));

                // upper left
                dev.Rasterizer.SetScissorRectangle(0, 0, scissorsPos.X, scissorsPos.Y);
                currentView.Draw(models.Pipelines[visible[0]].Image);

                // upper right
                dev.Rasterizer.SetScissorRectangle(scissorsPos.X, 0, size.Width, scissorsPos.Y);
                currentView.Draw(models.Pipelines[visible[1]].Image);

                // draw third texture (entire bottom if only 3 are visible, lower left if 4 are visible)
                dev.Rasterizer.SetScissorRectangle(0, scissorsPos.Y, visible.Count == 3 ? size.Width : scissorsPos.X, scissorsPos.Y);
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

            if (mouseDown)
            {
                // drag event
                // drag event
                var diff = new Point(newPosition.X - mousePosition.X, newPosition.Y - mousePosition.Y);
                if (Math.Abs(diff.X) > 0.01 || Math.Abs(diff.Y) > 0.01)
                {
                    currentView.OnDrag(new Vector2(diff.X, diff.Y));
                }
            }

            mousePosition = newPosition;

            models.Display.TexelPosition = currentView.GetTexelPosition(
                ConvertToCanonical(new Vector2(mousePosition.X, mousePosition.Y)));
        }

        private void DxHostOnMouseLeave(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void DxHostOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                mouseDown = false;
            mousePosition = new Point((int)e.GetPosition(dxHost).X, (int)e.GetPosition(dxHost).Y);
        }

        private void DxHostOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                mouseDown = true;

            mousePosition = new Point((int)e.GetPosition(dxHost).X, (int)e.GetPosition(dxHost).Y);
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

            models.Display.TexelPosition = currentView.GetTexelPosition(canMouse);
        }

        private Vector2 ConvertToCanonical(Vector2 windowCoord)
        {
            return new Vector2(
                (float)(windowCoord.X * 2.0 / models.Window.ClientSize.Width - 1.0),
                (float)(windowCoord.Y * -2.0 / models.Window.ClientSize.Height + 1.0)
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
                            default:
                                currentView = new EmptyView();
                                break;
                        }
                    }
                    catch (Exception err)
                    {
                        models.Window.ShowErrorDialog(err.Message);
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
