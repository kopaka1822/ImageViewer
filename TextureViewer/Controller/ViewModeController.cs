using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller.TextureViews;
using TextureViewer.Models;

namespace TextureViewer.Controller
{
    public class ViewModeController
    {
        private readonly Models.Models models;
        private ITextureView currentView = new EmptyView();
        private bool mouseDown = false;
        private Point mousePosition = new Point(0);

        public ViewModeController(Models.Models models)
        {
            this.models = models;
            models.Display.PropertyChanged += ViewModeModelOnPropertyChanged;
            models.GlContext.GlControl.MouseWheel += GlControlOnMouseWheel;
            models.GlContext.GlControl.MouseDown += GlControlOnMouseDown;
            models.GlContext.GlControl.MouseUp += GlControlOnMouseUp;
            models.GlContext.GlControl.MouseLeave += GlControlOnMouseLeave;
            models.GlContext.GlControl.MouseMove += GlControlOnMouseMove;
        }

        private void GlControlOnMouseLeave(object sender, EventArgs eventArgs)
        {
            mouseDown = false;
        }

        private void GlControlOnMouseUp(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
                mouseDown = false;
            mousePosition = new Point(args.X, args.Y);
        }

        private void GlControlOnMouseDown(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
                mouseDown = true;
            mousePosition = new Point(args.X, args.Y);
        }

        private Vector2 ConvertToCanonical(Vector2 windowCoord)
        {
            return new Vector2(
                (float)(windowCoord.X * 2.0 / models.GlContext.ClientSize.Width - 1.0),
                (float)(windowCoord.Y * -2.0 / models.GlContext.ClientSize.Height + 1.0)
            );
        }

        private void GlControlOnMouseWheel(object sender, MouseEventArgs mouseEventArgs)
        {
            // dont interrupt when processing
            if (models.Progress.IsProcessing) return;

            // convert to canonical coordinates
            currentView.OnScroll(
                (float)mouseEventArgs.Delta, 
                ConvertToCanonical(new Vector2((float)mouseEventArgs.X, (float)mouseEventArgs.Y))
            );

            models.Display.TexelPosition = currentView.GetTexelPosition(
                ConvertToCanonical(new Vector2((float)mouseEventArgs.X, (float)mouseEventArgs.Y)));

            models.GlContext.RedrawFrame();
        }

        private void GlControlOnMouseMove(object sender, MouseEventArgs args)
        {
            var newPosition = new Point(args.X, args.Y);

            // dont interrupt when processing
            if (models.Progress.IsProcessing) return;

            if (mouseDown)
            {
                // drag event
                var diff = new Point(newPosition.X - mousePosition.X, newPosition.Y - mousePosition.Y);
                if (Math.Abs(diff.X) > 0.01 || Math.Abs(diff.Y) > 0.01)
                {
                    currentView.OnDrag(new Vector2(diff.X, diff.Y));
                }
            }

            mousePosition = newPosition;

            models.Display.TexelPosition = currentView.GetTexelPosition(
                ConvertToCanonical(new Vector2((float)args.X, (float)args.Y)));

            // TODO only redraw on certain conditions
            models.GlContext.RedrawFrame();
        }

        private void ViewModeModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.ActiveView):
                    var disable = models.GlContext.Enable();

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

                        models.GlContext.RedrawFrame();
                    }
                    catch (Exception e)
                    {
                        App.ShowErrorDialog(models.App.Window, e.Message);
                    }
                    finally
                    {
                        if (disable) models.GlContext.Disable();
                    }                  
                    break;

                case nameof(DisplayModel.ActiveLayer):
                case nameof(DisplayModel.ActiveMipmap):
                case nameof(DisplayModel.Zoom):
                case nameof(DisplayModel.Aperture):
                    models.GlContext.RedrawFrame();
                    break;
            }
        }

        public void Paint()
        {
            // draw visible equations
            var visible = models.Equations.GetVisibles();
            if (visible.Count == 1)
            {
                // draw a single image
                currentView.Draw(models.FinalImages.Get(visible[0]).Texture);
            }
            else if (visible.Count == 2)
            {
                // draw two images in split view
                GL.Enable(EnableCap.ScissorTest);

                var clientX = models.GlContext.ClientSize.Width;
                var clientY = models.GlContext.ClientSize.Height;
                // clamp mouse position to avoid out of range
                var scissorsPos = new Point(mousePosition.X, mousePosition.Y);
                scissorsPos.X = Math.Min(clientX - 1, Math.Max(0, scissorsPos.X));
                scissorsPos.Y = Math.Min(clientY - 1, Math.Max(0, scissorsPos.Y));

                if(models.Display.Split == DisplayModel.SplitMode.Vertical)
                    GL.Scissor(0, 0, scissorsPos.X, clientY);
                else
                    GL.Scissor(0, clientY - scissorsPos.Y, clientX, clientY);

                // first part
                currentView.Draw(models.FinalImages.Get(visible[0]).Texture);

                if (models.Display.Split == DisplayModel.SplitMode.Vertical)
                    GL.Scissor(scissorsPos.X, 0, clientX, clientY);
                else
                    GL.Scissor(0, 0, clientX, clientY - scissorsPos.Y);

                // second part
                currentView.Draw(models.FinalImages.Get(visible[1]).Texture);

                GL.Disable(EnableCap.ScissorTest);
            }
        }
    }
}
