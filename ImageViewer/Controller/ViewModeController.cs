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
            models.Window.Repaint += WindowOnRepaint;
        }

        private void WindowOnRepaint(object sender, EventArgs e)
        {
            
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

            models.Window.RedrawFrame();
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

            models.Window.RedrawFrame();
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
                            default:
                                currentView = new EmptyView();
                                break;
                        }
                        
                        models.Window.RedrawFrame();
                    }
                    catch (Exception err)
                    {
                        models.Window.ShowErrorDialog(err.Message);
                    }
                    break;

                case nameof(DisplayModel.ActiveLayer):
                case nameof(DisplayModel.ActiveMipmap):
                case nameof(DisplayModel.Zoom):
                case nameof(DisplayModel.Aperture):
                    this.models.Window.RedrawFrame();
                    break;
            }
        }

        public void Dispose()
        {
            currentView?.Dispose();
        }
    }
}
