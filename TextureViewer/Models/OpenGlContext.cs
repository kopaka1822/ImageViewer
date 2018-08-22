using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Annotations;
using TextureViewer.ViewModels;
using TextureViewer.Views;

namespace TextureViewer.Models
{
    public class OpenGlContext : INotifyPropertyChanged
    {
        public struct Size
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

#if DEBUG
        private bool debugGl = true;
#else
        private bool debugGl = false;
#endif
        private bool issuedRedraw = false;

        public GLControl GlControl { get; }

        public int MaxTextureUnits { get; }

        public static int MajorVersion { get; } = 4;
        public static int MinorVersion { get; } = 3;
        public static string ShaderVersion { get; } = "#version 430 core";

        public OpenGlContext(MainWindow window, WindowViewModel viewModel)
        {
            try
            {
                var flags = GraphicsContextFlags.Default;
                if (debugGl)
                    flags |= GraphicsContextFlags.Debug;

                // init opengl
                GlControl = new GLControl(new GraphicsMode(new ColorFormat(32), 32), MajorVersion, MinorVersion, flags)
                {
                    Dock = DockStyle.Fill
                };

                window.OpenGlHost.Child = GlControl;

                // initialize client size
                window.Loaded += (sender, args) => 
                {
                    var source = PresentationSource.FromVisual(window);
                    var scalingX = source.CompositionTarget.TransformToDevice.M11;
                    var scalingY = source.CompositionTarget.TransformToDevice.M22;
                    // update client size
                    ClientSize = new Size()
                    {
                        Width = (int)(window.OpenGlHost.ActualWidth * scalingX),
                        Height = (int)(window.OpenGlHost.ActualHeight * scalingY)
                    };

                    // change size with callback
                    window.OpenGlHost.SizeChanged += (sender2, args2) =>
                    {
                        ClientSize = new Size()
                        {
                            Width = (int)(window.OpenGlHost.ActualWidth * scalingX),
                            Height = (int)(window.OpenGlHost.ActualHeight * scalingY)
                        };
                    };
                };

                // reset redraw state
                GlControl.Paint += (sender, args) => issuedRedraw = false;
                GlControl.DragOver += (o, args) => args.Effect = System.Windows.Forms.DragDropEffects.Copy;
                GlControl.AllowDrop = true;

                GlControl.ContextMenuStrip = new OpenGlHostContextMenu(viewModel);

                Enable();

                GL.Enable(EnableCap.TextureCubeMapSeamless);
                GL.Enable(EnableCap.FramebufferSrgb);
                GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
                MaxTextureUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
            }
            catch (Exception e)
            {
                Console.Write(e);
                throw;
            }
            finally
            {
                Disable();
            }
        }

        private Size clientSize = new Size(){Height = 0, Width = 0}; 
        public Size ClientSize
        {
            get => clientSize;
            private set
            {
                if (value.Width == clientSize.Width && value.Height == clientSize.Height) return;
                clientSize = value;
                OnPropertyChanged(nameof(ClientSize));
            }
        }

        private bool glEnabled = false;
        public bool GlEnabled => glEnabled;

        /// <summary>
        /// makes the window opengl context current
        /// </summary>
        /// <returns>true if the context was not enabled before</returns>
        public bool Enable()
        {
            var wasEnabled = GlEnabled;
            GlControl?.MakeCurrent();
            if (debugGl)
                glhelper.Utility.EnableDebugCallback();
            glEnabled = true;

            return !wasEnabled;
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
                GlControl?.Context.MakeCurrent(null);
            }
            catch (GraphicsContextException)
            {
                // happens sometimes..
            }

            glEnabled = false;
        }

        /// <summary>
        /// the frame will be redrawn as soon as possible
        /// </summary>
        public void RedrawFrame()
        {
            if (issuedRedraw) return;
            GlControl?.Invalidate();
            issuedRedraw = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
