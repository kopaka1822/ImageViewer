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
using TextureViewer.Models.Dialog;

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
            this.models.Export.PropertyChanged += ExportOnPropertyChanged;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;

            this.viewModeController = new ViewModeController(models);
            this.progressController = new ProgressController(models);
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch(args.PropertyName)
            {
                case nameof(DisplayModel.LinearInterpolation):
                case nameof(DisplayModel.ShowCropRectangle):
                    models.GlContext.RedrawFrame();
                    break;
            }
        }

        private void ExportOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ExportModel.CropStartX):
                case nameof(ExportModel.CropStartY):
                case nameof(ExportModel.CropEndX):
                case nameof(ExportModel.CropEndY):
                case nameof(ExportModel.UseCropping):
                case nameof(ExportModel.Mipmap):
                case nameof(ExportModel.Layer):
                case nameof(ExportModel.IsExporting):
                    models.GlContext.RedrawFrame();
                    break;
            }
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
                    // Advance to process (do work for 500 ms)
                    var timer = new System.Timers.Timer();
                    var timeUp = false;
                    timer.Elapsed += (source, args) =>
                    {
                        timeUp = true;
                    };
                    timer.Interval = 500;
                    timer.Enabled = true;

                    do
                    {
                        progressController.DoWork();
                        GL.Finish();
                    }
                    while (!timeUp && progressController.HasWork());
                    
                    models.GlContext.RedrawFrame();
                }
                else
                {
                    // draw the frame

                    GL.Viewport(0, 0, context.ClientSize.Width, context.ClientSize.Height);
                    GL.ClearColor(0.854992608124234f, 0.854992608124234f, 0.854992608124234f, 1.0f);
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
