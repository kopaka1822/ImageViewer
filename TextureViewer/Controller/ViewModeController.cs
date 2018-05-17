using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using TextureViewer.Controller.TextureViews;
using TextureViewer.Models;

namespace TextureViewer.Controller
{
    public class ViewModeController
    {
        private readonly Models.Models models;
        private ITextureView currentView = new EmptyView();
        private bool mouseDown = false;
        private Vector2 mousePosition = Vector2.Zero;

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
            mousePosition = new Vector2(args.X, args.Y);
        }

        private void GlControlOnMouseDown(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
                mouseDown = true;
            mousePosition = new Vector2(args.X, args.Y);
        }

        private void GlControlOnMouseWheel(object sender, MouseEventArgs mouseEventArgs)
        {
            currentView.OnScroll((float)mouseEventArgs.Delta, new Vector2((float)mouseEventArgs.X, (float)mouseEventArgs.Y));
            models.GlContext.RedrawFrame();
        }

        private void GlControlOnMouseMove(object sender, MouseEventArgs args)
        {
            var newPosition = new Vector2(args.X, args.Y);

            if (mouseDown)
            {
                // drag event
                var diff = newPosition - mousePosition;
                if (Math.Abs(diff.X) > 0.01 || Math.Abs(diff.Y) > 0.01)
                {
                    currentView.OnDrag(diff);
                }
            }

            mousePosition = newPosition;

            models.GlContext.RedrawFrame();
        }

        private void ViewModeModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.ActiveView):
                    var disable = models.GlContext.Enable();

                    currentView.Dispose();
                    currentView = null;
                    switch (models.Display.ActiveView)
                    {
                        case DisplayModel.ViewMode.Single:
                            currentView = new SingleTextureView(models);
                            break;
                        //case DisplayModel.ViewMode.CubeMap:
                            //break;
                        //case DisplayModel.ViewMode.Polar:
                            //break;
                        //case DisplayModel.ViewMode.CubeCrossView:
                            //break;
                        default:
                            currentView = new EmptyView();
                            break;
                    }

                    models.GlContext.RedrawFrame();
                    if(disable) models.GlContext.Disable();
                    break;
            }
        }

        public void Paint()
        {
            // draw visible equations
            // TODO correct this
            if (models.Equations.Get(0).Visible)
            {
                currentView.Draw(models.FinalImages.Get(0).Texture);
            }
        }
    }
}
