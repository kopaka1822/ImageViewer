using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
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
using OpenTKImageViewer.Dialogs;
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.ImageContext;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using ContextMenu = System.Windows.Forms.ContextMenu;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using WindowState = System.Windows.WindowState;

namespace OpenTKImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region VARIABLES
        public readonly App ParentApp;
        private GLControl glControl;

        private string error = "";
        private int iteration = 0;
        
        public ulong ZIndex { get; set; }
        private Dictionary<ImageViewType, IImageView> imageViews = new Dictionary<ImageViewType, IImageView>();
        private ImageViewType currentImageView;
        private ProgressWindow progressWindow = null;

        private Rect windowRect;

        public StatusBarControl StatusBar { get; }
        public ImageContext.ImageContext Context { get; set; }
        public ImageViewType CurrentView
        {
            get { return currentImageView; }
            set
            {
                if (imageViews.ContainsKey(value))
                {
                    currentImageView = value;
                    RedrawFrame();
                }
            }
        }

        public TonemapWindow TonemapDialog { get; set; } = null;
        public ImageWindow ImageDialog { get; set; } = null;

        public System.Boolean IsInteractionEnabled { get; set; } = true;
        #endregion

        #region INITIALIZATION

        public MainWindow(App parentApp, ImageContext.ImageContext context)
        {
            this.ParentApp = parentApp;
            this.Context = context;
            this.ZIndex = 0;
            
            InitializeComponent();
            windowRect = new Rect(Left, Top, Width, Height);

            this.Width = parentApp.GetConfig().WindowSizeX;
            this.Height = parentApp.GetConfig().WindowSizeY;
            if (parentApp.GetConfig().IsMaximized)
                WindowState = WindowState.Maximized;

            StatusBar = new StatusBarControl(this);
            CreateImageViews();

            // redraw if context changes
            context.ChangedMipmap += (sender, args) => RedrawFrame();
            
            context.ChangedImages += (sender, args) =>
            {
                RedrawFrame();
                if (context.GetNumImages() == 0)
                {
                    CreateImageViews();
                }
            };
            context.ChangedLayer += (sender, args) => RedrawFrame();
            context.ChangedFiltering += (sender, args) => RedrawFrame();
            context.ChangedGrayscale += (sender, args) => RedrawFrame();
            context.Tonemapper.ChangedSettings += (sender, args) => RedrawFrame();

            // set default values
            MenuItemLinearInterpolation.IsChecked = context.LinearInterpolation;
            context.ChangedFiltering += (sender, args) => MenuItemLinearInterpolation.IsChecked =
                context.LinearInterpolation;

            SetGrayscale(context.Grayscale);
            context.ChangedGrayscale += (sender, args) => SetGrayscale(context.Grayscale);

            context.ChangedMipmap += (sender, args) =>
            {
                EnableOpenGl();
                imageViews[currentImageView]?.UpdateMouseDisplay(this);
                DisableOpenGl();
            };
        }

        private void CreateImageViews()
        {
            imageViews.Clear();
            if (Context.GetNumImages() > 0)
            {
                imageViews.Add(ImageViewType.Single, new SingleView(Context, BoxScroll));
                CurrentView = ImageViewType.Single;
                if (Context.GetNumLayers() == 6)
                {
                    imageViews.Add(ImageViewType.CubeMap, new CubeView(Context, BoxScroll));
                    imageViews.Add(ImageViewType.CubeCrossView, new CubeCrossView(Context, BoxScroll));
                    CurrentView = ImageViewType.CubeMap;
                }
                else if (Context.GetNumLayers() == 1)
                {
                    imageViews.Add(ImageViewType.Polar, new PolarView(Context, BoxScroll));
                }
            }

            if (imageViews.Count == 0)
            {
                imageViews.Add(ImageViewType.Empty, new EmptyView());
                CurrentView = ImageViewType.Empty;
            }

            // update view box
            StatusBar.UpdateViewBox();
        }


        private void WinFormsHost_OnInitialized(object sender, EventArgs e)
        {
            try
            {
                var flags = GraphicsContextFlags.Default | GraphicsContextFlags.Debug;
                glControl = new GLControl(new GraphicsMode(32, 24), 4, 2, flags);
                glControl.Paint += GLControl_Paint;
                glControl.Dock = DockStyle.Fill;
                var windowsFormsHost = sender as WindowsFormsHost;
                if (windowsFormsHost != null) windowsFormsHost.Child = glControl;

                EnableOpenGl();

                GL.Enable(EnableCap.TextureCubeMapSeamless);
                GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                glControl.MouseMove += (o, args) => WinFormsHost_OnMouseMove(args);
                glControl.MouseWheel += (o, args) => WinFormsHost_OnMouseWheel(args);
                glControl.MouseDown += (o, args) => WinFormsHost_OnMouseDown(args);
                glControl.MouseUp += (o, args) => WinFormsHost_OnMouseUp(args);
                glControl.MouseLeave += (o, args) => WinFormsHost_OnMouseLeave(args);
                glControl.DragDrop += (o, args) => WinFormsHost_OnDrop(args);
                glControl.DragOver += (o, args) =>
                {
                    args.Effect = System.Windows.Forms.DragDropEffects.Copy;
                };
                glControl.AllowDrop = true;

                DisableOpenGl();

                // create context menu
                glControl.ContextMenuStrip = new ContextMenuStrip();
                var colorItem = glControl.ContextMenuStrip.Items.Add("Show Pixel Color");
                {
                    var sri = System.Windows.Application.GetResourceStream(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/eyedropper.png", UriKind.Absolute));
                    if(sri != null)
                        colorItem.Image = System.Drawing.Image.FromStream(sri.Stream);
                }
                colorItem.Click += (o, args) =>
                {
                    var color = StatusBar.GetCurrentPixelColor();
                    var dia = new PixelInformationWindow(color.X, color.Y, color.Z, color.W);
                    dia.ShowDialog();
                };

                var pixelDisplayItem = glControl.ContextMenuStrip.Items.Add("Pixel Display");
                {
                    var sri = System.Windows.Application.GetResourceStream(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/displayconfig.png", UriKind.Absolute));
                    if(sri != null)
                        pixelDisplayItem.Image = System.Drawing.Image.FromStream(sri.Stream);
                }
                pixelDisplayItem.Click += (o, args) => MenuItem_Click_PixelDisplay(o, null);

                var imageStatisitcsItem = glControl.ContextMenuStrip.Items.Add("Image Statistics");
                {
                    var sri = System.Windows.Application.GetResourceStream(new Uri($@"pack://application:,,,/{App.AppName};component/Icons/statistics.png", UriKind.Absolute));
                    if (sri != null)
                        imageStatisitcsItem.Image = System.Drawing.Image.FromStream(sri.Stream);
                }
                imageStatisitcsItem.Click += (o, args) =>
                {
                    var dia = new StatisticsWindow(this);
                    dia.Left = this.Left + 0.5 * (this.Width - dia.Width);
                    dia.Top = this.Top + 0.5 * (this.Height - dia.Height);
                    dia.ShowDialog();
                };
            }
            catch (Exception exception)
            {
                error = exception.Message + ": " + exception.StackTrace;
            }
        }

        private void DisableDebugCallback()
        {
            GL.Disable(EnableCap.DebugOutput);
        }

        /// <summary>
        /// the frame will be redrawn as soon as possible
        /// </summary>
        public void RedrawFrame()
        {
            glControl?.Invalidate();
        }

        #endregion

        #region OpenGL

        /// <summary>
        /// enables the opengl context from this window
        /// </summary>
        public void EnableOpenGl()
        {
            glControl?.MakeCurrent();
            glhelper.Utility.EnableDebugCallback();
        }

        public void DisableOpenGl()
        {
            GL.Flush();
            DisableDebugCallback();
            try
            {

                glControl?.Context.MakeCurrent(null);
            }
            catch (GraphicsContextException)
            {
                // happens sometimes..
            }
        }

        public double GetDpiScalingX()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            return source.CompositionTarget.TransformToDevice.M11;
        }

        public double GetDpiScalingY()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            return source.CompositionTarget.TransformToDevice.M22;
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (error.Length > 0 && iteration++ > 0)
            {
                App.ShowErrorDialog(this, error);
                error = "";
            }

            try
            {
                EnableOpenGl();
                glhelper.Utility.GLCheck();

                GL.Viewport(0, 0, (int)(GetClientWidth()), 
                    (int)(GetClientHeight()));
                GL.ClearColor(0.9333f, 0.9333f, 0.9333f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                StatusBar.InitOpenGl();
                if (Context.Update())
                {
                    // image is ready
                    glhelper.Utility.GLCheck();
                    if (progressWindow != null)
                    {
                        DisableOpenGl();
                        
                        // should be finished
                        progressWindow.SetProgress(1.0f);
                        progressWindow.Close();
                        progressWindow = null;
                        EnableWindowInteractions();

                        EnableOpenGl();
                    }

                    glhelper.Utility.GLCheck();
                    imageViews[CurrentView]?.Update(this);
                    glhelper.Utility.GLCheck();

                    if (Context.GetNumActiveImages() == 2)
                    {
                        // draw both images with scissor test
                        GL.Enable(EnableCap.ScissorTest);

                        // clamp mouse position to not go outside the window
                        var scissorPos = new Point(MousePosition.X, MousePosition.Y);
                        scissorPos.X = Math.Min(GetClientWidth()- 1.0f, Math.Max(0.0, scissorPos.X));
                        scissorPos.Y = Math.Min(GetClientWidth() - 1.0f, Math.Max(0.0, scissorPos.Y));

                        if (Context.SplitView == ImageContext.ImageContext.SplitViewMode.Vertical)
                            GL.Scissor(0, 0, (int)scissorPos.X, (int)GetClientHeight());
                        else
                            GL.Scissor(0, (int)GetClientHeight() - (int)scissorPos.Y, (int)GetClientWidth(), (int)GetClientHeight());

                        imageViews[CurrentView]?.Draw(0);

                        if (Context.SplitView == ImageContext.ImageContext.SplitViewMode.Vertical)
                            GL.Scissor((int)scissorPos.X, 0, (int)GetClientWidth(), (int)GetClientHeight());
                        else
                            GL.Scissor(0, 0, (int)GetClientWidth(), (int)GetClientHeight() - (int)scissorPos.Y);

                        imageViews[CurrentView]?.Draw(1);

                        GL.Disable(EnableCap.ScissorTest);
                    }
                    else if(Context.GetNumActiveImages() == 1)
                    {
                        // draw the active image without scissor testing
                        int activeImage = (Context.GetImageConfiguration(0).Active ? 0 : 1);
                        imageViews[CurrentView]?.Draw(activeImage);
                    }
                    glControl.SwapBuffers();
                }
                else
                {
                    glhelper.Utility.GLCheck();
                    if (progressWindow == null)
                    {
                        DisableOpenGl();
                        
                        progressWindow = new ProgressWindow();
                        progressWindow.Show();

                        progressWindow.SetDescription("applying tonemappers");

                        progressWindow.Abort += (o, args) =>
                        {
                            Context.AbortImageProcessing();
                            App.ShowInfoDialog((TonemapDialog==null)?(Window)this:(Window)TonemapDialog, "Operation aborted. The displayed picture may contain errors.");
                            EnableWindowInteractions();
                        };


                        // disable all interactions with open windows
                        DisableWindowInteractions();
                        EnableOpenGl();
                    }
                    
                    // image is not ready yet
                    glhelper.Utility.GLCheck();
                    progressWindow?.SetProgress(Context.GetImageProcess());
                    progressWindow?.SetDescription(Context.GetImageLoadingDescription());

                    RedrawFrame();
                    GL.Finish();
                }
            }
            catch (Exception exception)
            {
#if DEBUG
                Debugger.Break();
#endif
                if (error.Length == 0)
                    error = exception.Message + ": " + exception.StackTrace;
            }
            DisableOpenGl();
        }


        private void WinFormsHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawFrame();
        }

        #endregion

        #region WINDOW INTERACTION

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ParentApp.GetConfig().IsMaximized = WindowState == WindowState.Maximized;
            if (WindowState != WindowState.Maximized)
            {

                ParentApp.GetConfig().WindowSizeX = (int)Width;
                ParentApp.GetConfig().WindowSizeY = (int)Height;
            }

            // put dialogs back on screen of they were shifted out by OnLocationChange
            if (ImageDialog != null)
                ShiftWindowOntoScreen(ImageDialog);
            if (TonemapDialog != null)
                ShiftWindowOntoScreen(TonemapDialog);
        }

        private void DisableWindowInteractions()
        {
            if (TonemapDialog != null)
                TonemapDialog.IsEnabled = false;

            if (ImageDialog != null)
                ImageDialog.IsEnabled = false;
                
            //IsEnabled = false;
            IsInteractionEnabled = false;
        }

        private void EnableWindowInteractions()
        {
            if (TonemapDialog != null)
                TonemapDialog.IsEnabled = true;

            if (ImageDialog != null)
                ImageDialog.IsEnabled = true;
            
            //IsEnabled = true;
            IsInteractionEnabled = true;
        }

        private void WinFormsHost_OnDrop(System.Windows.Forms.DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])args.Data.GetData(DataFormats.FileDrop);

                if (files != null)
                    foreach (var file in files)
                        ImportOrOpenImage(file);
            }
        }

        // mouse tracking
        public Point MousePosition { get; private set; } = new Point();
        private bool mouseDown = false;

        private void WinFormsHost_OnMouseMove(System.Windows.Forms.MouseEventArgs args)
        {
            var newPosition = new Point(args.X, args.Y);
            // clamp values
            //newPosition.X = Math.Min(GetClientWidth(), Math.Max(0.0, newPosition.X));
            //newPosition.Y = Math.Min(GetClientWidth(), Math.Max(0.0, newPosition.Y));

            if (mouseDown)
            {
                // drag event
                var diff = newPosition - MousePosition;

                if (Math.Abs(diff.X) > 0.01 || Math.Abs(diff.Y) > 0.01)
                {
                    imageViews[currentImageView]?.OnDrag(diff, this);
                    RedrawFrame();
                }
            }

            MousePosition = newPosition;

            EnableOpenGl();
            imageViews[CurrentView]?.UpdateMouseDisplay(this);
            DisableOpenGl();

            // redraw frame on mouse move when viewing 2 pictures on the same time
            if(Context.GetNumActiveImages() == 2)
                RedrawFrame();
        }

        private void WinFormsHost_OnMouseDown(System.Windows.Forms.MouseEventArgs args)
        {
            if(args.Button == MouseButtons.Left)
                mouseDown = true;
            MousePosition = new Point(args.X, args.Y);
        }

        private void WinFormsHost_OnMouseUp(System.Windows.Forms.MouseEventArgs args)
        {
            if(args.Button == MouseButtons.Left)
                mouseDown = false;
            MousePosition = new Point(args.X, args.Y);
        }

        private void WinFormsHost_OnMouseLeave(System.EventArgs args)
        {
            mouseDown = false;
        }

        private void WinFormsHost_OnMouseWheel(System.Windows.Forms.MouseEventArgs args)
        {
            imageViews[currentImageView]?.OnScroll(args.Delta, new Point(args.X, args.Y));
            EnableOpenGl();
            imageViews[CurrentView]?.UpdateMouseDisplay(this);
            DisableOpenGl();
            RedrawFrame();
        }
        
        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Context.Tonemapper.HasKeyToInvoke(e.Key))
            {
                // if tonemapper dialog is open, apply tonemapper dialog and then apply settings
                if (TonemapDialog != null)
                {
                    TonemapDialog.GetCurrentSettings().InvokeKey(e.Key);
                    Context.Tonemapper.Apply(TonemapDialog.GetCurrentSettings());
                }
                else
                {
                    // just invoke key
                    Context.Tonemapper.InvokeKey(e.Key);
                }
            }
            else // no keybinding key to invoke
            {
                // standart keybindings
                switch (e.Key)
                {
                    case Key.Right:
                        Context.ActiveLayer += 1;
                        break;
                    case Key.Left:
                        Context.ActiveLayer -= 1;
                        break;
                    case Key.Up:
                        Context.ActiveMipmap -= 1;
                        break;
                    case Key.Down:
                        Context.ActiveMipmap += 1;
                        break;
                    default:
                        // nothing was changed
                        return;
                }
            }

            e.Handled = true;
            RedrawFrame();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            TonemapDialog?.Close();
            ImageDialog?.Close();
            ParentApp.UnregisterWindow(this);
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            if (TonemapDialog != null)
                TonemapDialog.Topmost = true;

            if (ImageDialog != null)
                ImageDialog.Topmost = true;

            ParentApp.SetActiveWindow(this);
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            if (TonemapDialog != null)
                TonemapDialog.Topmost = false;

            if (ImageDialog != null)
                ImageDialog.Topmost = false;
        }

        private void MainWindow_OnLocationChanged(object sender, EventArgs e)
        {
            var newRect = new Rect(Left, Top, Width, Height);

            bool isResize = newRect.Width != windowRect.Width || newRect.Height != windowRect.Height;
            if(!isResize)
            {
                double dx = newRect.X - windowRect.X;
                double dy = newRect.Y - windowRect.Y;

                // move dialogs
                if (TonemapDialog != null)
                    MoveDialog(TonemapDialog, dx, dy);

                if (ImageDialog != null)
                    MoveDialog(ImageDialog, dx, dy);
            }
            

            windowRect = newRect;
        }

        private void MoveDialog(Window dialog, double dx, double dy)
        {
            dialog.Left += dx;
            dialog.Top += dy;
        }

        #endregion

        public float GetClientWidth()
        {
            return (float)(WinFormsHost.ActualWidth * GetDpiScalingX());
        }

        public float GetClientHeight()
        {
            return (float)(WinFormsHost.ActualHeight * GetDpiScalingY());
        }


        #region MENU ITEMS

        #region FILE

        private void ImportOrOpenImage(string filename)
        {
            List<ImageLoader.Image> images;
            try
            {
                images = ImageLoader.LoadImage(filename);
            }
            catch (Exception e)
            {
                App.ShowErrorDialog(this, e.Message);
                return;
            }

            // import if format is correct
            try
            {
                ImportImage(images);
            }
            catch (Exception)
            {
                ParentApp.SpawnWindow(images);
            }
        }

        /// <summary>
        /// tries to import the image. throws error on failure
        /// </summary>
        /// <param name="images"></param>
        private void ImportImage(List<ImageLoader.Image> images)
        {
            bool isFirstImage = Context.GetNumImages() == 0;
            foreach (var image in images)
            {
                Context.AddImage(image);
            }
            
            HandleImageAdd(isFirstImage);
        }

        /// <summary>
        /// this should be called after an image was added.
        /// This will open an image dialog if multiple images are now present
        /// and a tonemapper dialog if it is the first image and the first
        /// image is a hdr image
        /// </summary>
        /// <param name="isFirstImage"></param>
        public void HandleImageAdd(bool isFirstImage)
        {
            if (Context.GetNumImages() > 1)
                ShowImagesWindow();

            if (isFirstImage)
            {
                imageViews.Clear();
                CreateImageViews();
                if (Context.HasHdr())
                {
                    // open tonemapper with gamma shader
                    bool wasOpen = TonemapDialog != null;
                    ShowTonemapper();


                    // try to apply the tonemapper
                    if (TonemapDialog.LoadTonemapper(ParentApp.ExecutionPath + "\\Tonemapper\\gamma.comp"))
                    {
                        // apply
                        TonemapDialog.ApplyTonemapper();
                    }

                    if(!wasOpen)
                        TonemapDialog.Close();

                    
                }
            }
        }

        /// <summary>
        /// tries to import the image. displays dialog on failure
        /// </summary>
        /// <param name="filename"></param>
        private void ImportImage(string filename)
        {
            try
            {
                var images = ImageLoader.LoadImage(filename);
                ImportImage(images);
            }
            catch (Exception exception)
            {
                App.ShowErrorDialog(this, exception.Message);
            }
        }

        private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = false;
            ofd.InitialDirectory = ParentApp.GetImagePath(ofd);

            if (ofd.ShowDialog() == true)
            {
                ParentApp.SetImagePath(ofd);
                if (Context.GetNumImages() == 0)
                {
                    ImportImage(ofd.FileName);
                }
                else
                {
                    ParentApp.SpawnWindow(ofd.FileName);
                }

            }
        }

        private void MenuItem_Click_Import(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = false;
            ofd.InitialDirectory = ParentApp.GetImagePath(ofd);

            if (ofd.ShowDialog() != true) return;
            // load image and import if possible
            ParentApp.SetImagePath(ofd);
            ImportImage(ofd.FileName);
        }

        private void MenuItem_Click_Export(object sender, RoutedEventArgs e)
        {
            if (Context.GetNumImages() == 0)
                return;

            // make sure only one imag is visible
            while (Context.GetNumActiveImages() == 2)
            {
                ShowImagesWindow();
                var res = MessageBox.Show(this,
                    "Two images are marked visible in the Image Dialog. Please mark only one image as visible when exporting.",
                    "Info",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);
                if (res == MessageBoxResult.Cancel)
                    return;
            }
            var activeImageId = Context.GetFirstActiveTexture();
            if (activeImageId == -1)
            {
                App.ShowErrorDialog(this, "No image is marked visible");
                return;
            }

            // open save file dialog
            Microsoft.Win32.SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp|HDR (*.hdr)|*.hdr";
            sfd.InitialDirectory = ParentApp.GetExportPath(sfd);
            if (sfd.ShowDialog() == false)
                return;

            ParentApp.SetExportPath(sfd);
            // obtain format
            ExportWindow.FileFormat format = ExportWindow.FileFormat.Png;
            if (sfd.FileName.EndsWith(".bmp"))
                format = ExportWindow.FileFormat.Bmp;
            else if (sfd.FileName.EndsWith(".hdr"))
                format = ExportWindow.FileFormat.Hdr;

            OpenTK.Graphics.OpenGL4.PixelFormat defaultFormat = OpenTK.Graphics.OpenGL4.PixelFormat.Rgb;
            if (Context.HasAlpha())
                defaultFormat = OpenTK.Graphics.OpenGL4.PixelFormat.Rgba;
            else if (Context.HasOnlyGrayscale())
                defaultFormat = OpenTK.Graphics.OpenGL4.PixelFormat.Red;

            // open dialog
            ExportWindow ew = new ExportWindow(this, sfd.FileName, format, defaultFormat);
            if (ew.ShowDialog() == false)
                return;

            // do the export
            EnableOpenGl();
            try
            {
                int width;
                int height;
                var data = Context.GetCurrentImageData(activeImageId, ew.SelectedMipmap, ew.SelectedLayer, ew.SelectedFormat,
                    ew.SelectedPixelType, out width, out height);

                if (data == null)
                {
                    App.ShowErrorDialog(this, "error retrieving file from gpu");
                    return;
                }

                switch (format)
                {
                    case ExportWindow.FileFormat.Png:
                        ImageLoader.SavePng(sfd.FileName, width, height, TextureArray2D.GetPixelFormatCount(ew.SelectedFormat), data);
                        break;
                    case ExportWindow.FileFormat.Bmp:
                        ImageLoader.SaveBmp(sfd.FileName, width, height, TextureArray2D.GetPixelFormatCount(ew.SelectedFormat), data);
                        break;
                    case ExportWindow.FileFormat.Hdr:
                        ImageLoader.SaveHdr(sfd.FileName, width, height, TextureArray2D.GetPixelFormatCount(ew.SelectedFormat), data);
                        break;
                }
            }
            catch (Exception exception)
            {
                App.ShowErrorDialog(this, exception.Message);
            }
            DisableOpenGl();
        }

        #endregion

        #region VIEW

        private void MenuItem_OnChecked_LinearFiltering(object sender, RoutedEventArgs e)
        {
            Context.LinearInterpolation = MenuItemLinearInterpolation.IsChecked;
        }

        private void SetGrayscale(ImageContext.ImageContext.GrayscaleMode mode)
        {
            switch (mode)
            {
                case ImageContext.ImageContext.GrayscaleMode.Disabled:
                    UpdateGrayscale(MenuItemGrayscaleDisabled);
                    break;
                case ImageContext.ImageContext.GrayscaleMode.Red:
                    UpdateGrayscale(MenuItemGrayscaleRed);
                    break;
                case ImageContext.ImageContext.GrayscaleMode.Green:
                    UpdateGrayscale(MenuItemGrayscaleGreen);
                    break;
                case ImageContext.ImageContext.GrayscaleMode.Blue:
                    UpdateGrayscale(MenuItemGrayscaleBlue);
                    break;
                case ImageContext.ImageContext.GrayscaleMode.Alpha:
                    UpdateGrayscale(MenuItemGrayscaleAlpha);
                    break;
            }
        }

        private void MenuItem_OnChecked_Grayscale(object sender, RoutedEventArgs e)
        {
            UpdateGrayscale(e.Source);
        }

        private void UpdateGrayscale(object sender)
        {
            MenuItemGrayscaleDisabled.IsCheckable = true;
            MenuItemGrayscaleRed.IsCheckable = true;
            MenuItemGrayscaleGreen.IsCheckable = true;
            MenuItemGrayscaleBlue.IsCheckable = true;
            MenuItemGrayscaleAlpha.IsCheckable = true;

            // Determine which was checked.
            if (Equals(sender, MenuItemGrayscaleDisabled))
            {
                MenuItemGrayscaleDisabled.IsCheckable = false;
                MenuItemGrayscaleRed.IsChecked = false;
                MenuItemGrayscaleGreen.IsChecked = false;
                MenuItemGrayscaleBlue.IsChecked = false;
                MenuItemGrayscaleAlpha.IsChecked = false;
            }
            else if (Equals(sender, MenuItemGrayscaleRed))
            {
                MenuItemGrayscaleRed.IsCheckable = false;
                MenuItemGrayscaleDisabled.IsChecked = false;
                MenuItemGrayscaleGreen.IsChecked = false;
                MenuItemGrayscaleBlue.IsChecked = false;
                MenuItemGrayscaleAlpha.IsChecked = false;
            }
            else if (Equals(sender, MenuItemGrayscaleGreen))
            {
                MenuItemGrayscaleGreen.IsCheckable = false;
                MenuItemGrayscaleRed.IsChecked = false;
                MenuItemGrayscaleDisabled.IsChecked = false;
                MenuItemGrayscaleBlue.IsChecked = false;
                MenuItemGrayscaleAlpha.IsChecked = false;
            }
            else if (Equals(sender, MenuItemGrayscaleBlue))
            {
                MenuItemGrayscaleBlue.IsCheckable = false;
                MenuItemGrayscaleRed.IsChecked = false;
                MenuItemGrayscaleGreen.IsChecked = false;
                MenuItemGrayscaleDisabled.IsChecked = false;
                MenuItemGrayscaleAlpha.IsChecked = false;
            }
            else if (Equals(sender, MenuItemGrayscaleAlpha))
            {
                MenuItemGrayscaleAlpha.IsCheckable = false;
                MenuItemGrayscaleRed.IsChecked = false;
                MenuItemGrayscaleGreen.IsChecked = false;
                MenuItemGrayscaleBlue.IsChecked = false;
                MenuItemGrayscaleDisabled.IsChecked = false;
            }

            if (MenuItemGrayscaleDisabled.IsChecked)
                Context.Grayscale = ImageContext.ImageContext.GrayscaleMode.Disabled;
            else if (MenuItemGrayscaleRed.IsChecked)
                Context.Grayscale = ImageContext.ImageContext.GrayscaleMode.Red;
            else if (MenuItemGrayscaleGreen.IsChecked)
                Context.Grayscale = ImageContext.ImageContext.GrayscaleMode.Green;
            else if (MenuItemGrayscaleBlue.IsChecked)
                Context.Grayscale = ImageContext.ImageContext.GrayscaleMode.Blue;
            else if (MenuItemGrayscaleAlpha.IsChecked)
                Context.Grayscale = ImageContext.ImageContext.GrayscaleMode.Alpha;
        }

        private void MenuItem_Click_PixelDisplay(object sender, RoutedEventArgs e)
        {
            var dia = new PixelDisplayWindow(this);
            dia.ShowDialog();
        }

        #endregion

        #region WINDOWS

        private void MenuItem_Click_Images(object sender, RoutedEventArgs e)
        {
            ShowImagesWindow();
        }

        private void ShowImagesWindow()
        {
            bool wasNull = ImageDialog == null;
            if (ImageDialog == null)
                ImageDialog = new ImageWindow(this);
            ImageDialog.Show();
            ImageDialog.Activate();
            if (wasNull)
            {
                ImageDialog.Left = Left + ParentApp.GetConfig().LastImageDialogX;
                ImageDialog.Top = Top + ParentApp.GetConfig().LastImageDialogY;
            }
            ShiftWindowOntoScreen(ImageDialog);
        }

        private void MenuItem_Click_Tonemapper(object sender, RoutedEventArgs e)
        {
            ShowTonemapper();
        }

        private void ShowTonemapper()
        {
            bool wasNull = TonemapDialog == null;
            if (TonemapDialog == null)
                TonemapDialog = new TonemapWindow(this);
            TonemapDialog.Show();
            TonemapDialog.Activate();
            if(wasNull)
            {
                TonemapDialog.Left = Left + ParentApp.GetConfig().LastTonemapDialogX;
                TonemapDialog.Top = Top + ParentApp.GetConfig().LastTonemapDialogY;
            }
            ShiftWindowOntoScreen(TonemapDialog);
        }

        #endregion

        #endregion

        #region DIALOG HELPER

        public ListBoxItem[] GenerateMipMapItems()
        {
            var items = new ListBoxItem[Context.GetNumMipmaps()];
            // generate mip map previews
            for (int curMipmap = 0; curMipmap < Context.GetNumMipmaps(); ++curMipmap)
            {
                items[curMipmap] = new ListBoxItem { Content = Context.GetWidth(curMipmap).ToString() + "x" + Context.GetHeight(curMipmap).ToString() };
            }
            return items;
        }

        public ListBoxItem[] GenerateLayerItems()
        {
            var items = new ListBoxItem[Context.GetNumLayers()];
            for (int curLayer = 0; curLayer < Context.GetNumLayers(); ++curLayer)
            {
                items[curLayer] = new ListBoxItem { Content = "Layer " + curLayer };
            }
            return items;
        }

        public List<ImageViewType> GetAvailableViews()
        {
            List<ImageViewType> res = new List<ImageViewType>();
            foreach (var imageView in imageViews)
            {
                res.Add(imageView.Key);
            }
            return res;
        }

        #endregion

        #region STATUS BAR 
        private void BoxScroll_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.Return)
            {
                if (imageViews.Count == 0)
                    return; // not yet initialized

                var text = BoxScroll.Text;
                if (text.EndsWith("%") || text.EndsWith("°"))
                    text = text.Substring(0, text.Length - 1);

                decimal dec;
                if (Decimal.TryParse(text, NumberStyles.Any, App.GetCulture(), out dec))
                {
                    imageViews[CurrentView]?.SetZoom((float)dec);
                    RedrawFrame();
                }
                e.Handled = true;
            }
        }

        #endregion


        protected override void OnClosed(EventArgs e)
        {
            // delete all opengl resources
            EnableOpenGl();

            DisposeViews();
            Context.Dispose();
            StatusBar.Dispose();
            
            DisableOpenGl();
            glControl.Dispose();
            base.OnClosed(e);
        }

        private void DisposeViews()
        {
            foreach (var imageView in imageViews)
            {
                imageView.Value.Dispose();
            }
        }

        public static void ShiftWindowOntoScreen(Window window)
        {
            // Note that "window.BringIntoView()" does not work.                            
            if (window.Top < SystemParameters.VirtualScreenTop)
            {
                window.Top = SystemParameters.VirtualScreenTop;
            }

            if (window.Left < SystemParameters.VirtualScreenLeft)
            {
                window.Left = SystemParameters.VirtualScreenLeft;
            }

            if (window.Left + window.Width > SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth)
            {
                window.Left = SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - window.Width;
            }

            if (window.Top + window.Height > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight)
            {
                window.Top = SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop - window.Height;
            }
        }
        }
}
