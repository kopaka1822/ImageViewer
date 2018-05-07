using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Models;

namespace TextureViewer.Controller
{
    public class PaintController
    {
        private readonly Models.Models models;
        private readonly ViewModeController viewModeController;
        private readonly ProgressController progressController;

        public PaintController(Models.Models models)
        {
            this.models = models;

            this.models.GlContext.GlControl.Paint += OnPaint;
            this.models.GlContext.PropertyChanged += GlContextOnPropertyChanged;

            this.viewModeController = new ViewModeController(models);
            this.progressController = new ProgressController(models);
        }

        private void GlContextOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(OpenGlContext.ClientSize):
                    models.GlContext.RedrawFrame();
                    break;
            }
        }

        private void OnPaint(object sender, PaintEventArgs paintEventArgs)
        {
            var context = models.GlContext;
            context.Enable();

            try
            {
                if (progressController.HasWork())
                {
                    // Advance to process

                    progressController.DoWork();
                    GL.Finish();
                    models.GlContext.RedrawFrame();
                }
                else
                {
                    // draw the frame

                    GL.Viewport(0, 0, context.ClientSize.Width, context.ClientSize.Height);
                    GL.ClearColor(0.9333f, 0.9333f, 0.9333f, 1.0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    //models.GlData.CheckersShader.Bind(Matrix4.Identity);
                    //models.GlData.Vao.DrawQuad();
                    viewModeController.Paint();

                    context.GlControl.SwapBuffers();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                context.Disable();
            }
        }
    }
}
