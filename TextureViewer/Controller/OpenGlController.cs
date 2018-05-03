using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Models;
using DragEventArgs = System.Windows.Forms.DragEventArgs;

namespace TextureViewer.Controller
{
    public class OpenGlController
    {
        private bool debugGl = true;
        private readonly GLControl glControl;
        private readonly MainWindow window;
        private readonly ViewModeModel viewModeModel = new ViewModeModel();
        private readonly ImagesModel imagesModel;

        public bool IsEnabled { get; private set; } = false;

        public OpenGlModel Model { get; private set; }

        public OpenGlController(MainWindow window, ImagesModel imagesModel)
        {
            this.window = window;
            this.imagesModel = imagesModel;
            imagesModel.PropertyChanged += ImagesModelOnPropertyChanged;

            try
            {
                var flags = GraphicsContextFlags.Default;
                if (debugGl)
                    flags |= GraphicsContextFlags.Debug;

                // init opengl for version 4.2
                glControl = new GLControl(new GraphicsMode(new ColorFormat(32), 32), 4, 2, flags)
                {
                    Dock = DockStyle.Fill
                };
                glControl.Paint += Paint;

                window.OpenGlHost.Child = glControl;
                window.OpenGlHost.SizeChanged += OpenGlHostOnSizeChanged;

                glControl.MouseMove += OnMouseMove;
                glControl.MouseWheel += OnMouseWheel;
                glControl.MouseDown += OnMouseDown;
                glControl.MouseUp += OnMouseUp;
                glControl.MouseLeave += OnMouseLeave;
                glControl.DragDrop += OnDragDrop;
                glControl.DragOver += (o, args) => args.Effect = System.Windows.Forms.DragDropEffects.Copy;
                glControl.AllowDrop = true;

                // set initial aspect ratio
                viewModeModel.AspectRatio = CalculateAspectRatio();

                Enable();

                GL.Enable(EnableCap.TextureCubeMapSeamless);
                GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                // create data model
                Model = new OpenGlModel(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Disable();
            }
        }

        private void OpenGlHostOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            RedrawFrame();
            viewModeModel.AspectRatio = CalculateAspectRatio();
        }

        private void ImagesModelOnPropertyChanged(object sender1, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    // recalculate if image dimensions change
                    viewModeModel.AspectRatio = CalculateAspectRatio();
                    break;
            }
        }

        public void Dispose()
        {
            Enable();

            try
            {
                Model?.Dispose();
                Model = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Disable();
            }
        }

        #region Input

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            
        }

        private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
        }

        private void OnMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
        }

        private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            

        }

        #endregion

        private void Paint(object sender, PaintEventArgs paintEventArgs)
        {
            try
            {
                Enable();

                GL.Viewport(0, 0, GetOpenGlHostWidth(), GetOpenGlHostHeight());
                GL.ClearColor(0.9333f, 0.9333f, 0.9333f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                Model.CheckersShader.Bind(Matrix4.Identity);
                Model.Vao.DrawQuad();

                glControl.SwapBuffers();
                //RedrawFrame();
                //GL.Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                Disable();
            }
        }

        /// <summary>
        /// makes the window opengl context current
        /// </summary>
        public void Enable()
        {
            glControl?.MakeCurrent();
            if(debugGl)
                glhelper.Utility.EnableDebugCallback();
            IsEnabled = true;
        }

        /// <summary>
        /// flushes commands and makes a null opengl context current
        /// </summary>
        public void Disable()
        {
            GL.Flush();
            if (debugGl)
                GL.Disable(EnableCap.DebugOutput);

            try
            {
                glControl?.Context.MakeCurrent(null);
            }
            catch (GraphicsContextException)
            {
                // happens sometimes..
            }

            IsEnabled = false;
        }

        /// <summary>
        /// the frame will be redrawn as soon as possible
        /// </summary>
        public void RedrawFrame()
        {
            glControl?.Invalidate();
        }

        /// <summary>
        /// actual width in pixels
        /// </summary>
        /// <returns></returns>
        private int GetOpenGlHostWidth()
        {
            PresentationSource source = PresentationSource.FromVisual(window);
            var scaling = source.CompositionTarget.TransformToDevice.M11;
            return (int)(window.OpenGlHost.ActualWidth * scaling);
        }

        /// <summary>
        /// actual height in pixels
        /// </summary>
        /// <returns></returns>
        private int GetOpenGlHostHeight()
        {
            PresentationSource source = PresentationSource.FromVisual(window);
            var scaling = source.CompositionTarget.TransformToDevice.M22;
            return (int)(window.OpenGlHost.ActualHeight * scaling);
        }

        private Matrix4 CalculateAspectRatio()
        {
            // images present?
            if (imagesModel.NumImages == 0)
                return Matrix4.Identity;

            return Matrix4.CreateScale(imagesModel.GetWidth(0) / (float)GetOpenGlHostWidth(), imagesModel.GetHeight(0) / (float)GetOpenGlHostHeight(), 1.0f);
        }
    }
}
