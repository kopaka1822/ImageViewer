using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool debugGl = true;
        private GLControl glControl;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void OpenGlHost_OnInitialized(object sender, EventArgs e)
        {
            try
            {
                var flags = GraphicsContextFlags.Default;
                if (debugGl)
                    flags |= GraphicsContextFlags.Debug;

                // init opengl for version 4.2
                glControl = new GLControl(new GraphicsMode(new ColorFormat(32), 32), 4, 2, flags);
                glControl.Paint += OpenGLHost_OnPaint;
                glControl.Dock = DockStyle.Fill;

                if (sender is WindowsFormsHost windowsFormsHost) windowsFormsHost.Child = glControl;

                glControl.MouseMove += OpenGlHost_OnMouseMove;
                glControl.MouseWheel += OpenGlHost_OnMouseWheel;
                glControl.MouseDown += OpenGlHost_OnMouseDown;
                glControl.MouseUp += OpenGlHost_OnMouseUp;
                glControl.MouseLeave += OpenGlHost_OnMouseLeave;
                glControl.DragDrop += OpenGlHost_OnDragDrop;
                glControl.DragOver += (o, args) => args.Effect = System.Windows.Forms.DragDropEffects.Copy;
                glControl.AllowDrop = true;

                // TODO create context menu

                EnableOpenGl();

                GL.Enable(EnableCap.TextureCubeMapSeamless);
                GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                DisableOpenGl();
            }
        }

        private void OpenGlHost_OnDragDrop(object sender, DragEventArgs e)
        {
            

        }

        private void OpenGlHost_OnMouseLeave(object sender, EventArgs e)
        {
            

        }

        private void OpenGlHost_OnMouseUp(object sender, MouseEventArgs e)
        {
            

        }

        private void OpenGlHost_OnMouseDown(object sender, MouseEventArgs e)
        {
            
        
        }

        private void OpenGlHost_OnMouseWheel(object sender, MouseEventArgs e)
        {
            

        }

        private void OpenGlHost_OnMouseMove(object sender, MouseEventArgs e)
        {
            

        }

        private void OpenGLHost_OnPaint(object sender, PaintEventArgs e)
        {
            // TODO proper error handling

            try
            {
                EnableOpenGl();

                GL.Viewport(0, 0, GetOpenGlHostWidth(), GetOpenGlHostHeight());
                GL.ClearColor(0.9333f, 0.9333f, 0.9333f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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
                DisableOpenGl();
            }
        }

        /// <summary>
        /// the frame will be redrawn as soon as possible
        /// </summary>
        public void RedrawFrame()
        {
            glControl?.Invalidate();
        }

        /// <summary>
        /// makes the window opengl context current
        /// </summary>
        public void EnableOpenGl()
        {
            glControl?.MakeCurrent();
            glhelper.Utility.EnableDebugCallback();
        }

        /// <summary>
        /// flushes commands and makes a null opengl context current
        /// </summary>
        public void DisableOpenGl()
        {
            GL.Flush();
            if(debugGl)
                GL.Disable(EnableCap.DebugOutput);

            try
            {
                glControl?.Context.MakeCurrent(null);
            }
            catch (GraphicsContextException)
            {
                // happens sometimes..
            }
        }

        /// <summary>
        /// actual width in pixels
        /// </summary>
        /// <returns></returns>
        private int GetOpenGlHostWidth()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            var scaling = source.CompositionTarget.TransformToDevice.M11;
            return (int)(OpenGlHost.ActualWidth * scaling);
        }

        /// <summary>
        /// actual height in pixels
        /// </summary>
        /// <returns></returns>
        private int GetOpenGlHostHeight()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            var scaling = source.CompositionTarget.TransformToDevice.M22;
            return (int)(OpenGlHost.ActualHeight * scaling);
        }

        private void OpenGlHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawFrame();
        }
    }
}
