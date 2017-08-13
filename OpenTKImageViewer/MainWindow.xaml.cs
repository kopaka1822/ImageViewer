using System;
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
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace OpenTKImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region VARIABLES
        private readonly App parent;
        private GLControl glControl;

        private string error = "";
        private int iteration = 0;
        
        public ulong ZIndex { get; set; }
        private Dictionary<ImageViewType, IImageView> imageViews = new Dictionary<ImageViewType, IImageView>();
        private ImageViewType currentImageView;
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

        #endregion

        #region INITIALIZATION

        public MainWindow(App parent, ImageContext.ImageContext context)
        {
            this.parent = parent;
            this.Context = context;
            this.ZIndex = 0;

            InitializeComponent();

            StatusBar = new StatusBarControl(this);
            CreateImageViews();


            // redraw if context changes
            context.ChangedMipmap += (sender, args) => RedrawFrame();
            context.ImageFormula1.Changed += (sender, args) => RedrawFrame();
            context.ChangedImages += (sender, args) => RedrawFrame();
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

            context.ChangedMipmap += (sender, args) => imageViews[currentImageView]?.UpdateMouseDisplay(this);
        }

        private void CreateImageViews()
        {
            if (Context.GetNumImages() > 0)
            {
                imageViews.Add(ImageViewType.Single, new SingleView(Context, BoxScroll));
                CurrentView = ImageViewType.Single;
                if (Context.GetNumLayers() == 6)
                {
                    imageViews.Add(ImageViewType.CubeMap, new CubeView(Context));
                    CurrentView = ImageViewType.CubeMap;
                }
                else if (Context.GetNumLayers() == 1)
                {
                    imageViews.Add(ImageViewType.Polar, new PolarView(Context));
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

                glControl.MakeCurrent();
                EnableDebugCallback();
                GL.Enable(EnableCap.TextureCubeMapSeamless);

                glControl.MouseMove += (o, args) => WinFormsHost_OnMouseMove(args);
                glControl.MouseWheel += (o, args) => WinFormsHost_OnMouseWheel(args);
                glControl.MouseDown += (o, args) => WinFormsHost_OnMouseDown(args);
                glControl.MouseUp += (o, args) => WinFormsHost_OnMouseUp(args);
                glControl.MouseLeave += (o, args) => WinFormsHost_OnMouseLeave(args);
            }
            catch (Exception exception)
            {
                error = exception.Message + ": " + exception.StackTrace;
            }
        }

        private void EnableDebugCallback()
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.Arb.DebugMessageCallback(OpenGlDebug, IntPtr.Zero);
            GL.Arb.DebugMessageControl(All.DontCare, All.DontCare, All.DontCare, 0, new int[0], true);
        }

        public static void OpenGlDebug(DebugSource source, DebugType type, int id, DebugSeverity severity, int length,
            IntPtr message, IntPtr userParam)
        {
            //string str = Marshal.PtrToStringAuto(message, length);
            string str = Marshal.PtrToStringAnsi(message, length);
            MessageBox.Show(str);
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
                glControl.MakeCurrent();
                EnableDebugCallback();

                GL.Viewport(0, 0, (int)(GetClientWidth()), 
                    (int)(GetClientHeight()));
                GL.ClearColor(0.9333f, 0.9333f, 0.9333f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                
                imageViews[CurrentView]?.Update(this);
                Context.Update();

                imageViews[CurrentView]?.Draw();

                Utility.GLCheck();
                glControl.SwapBuffers();
            }
            catch (Exception exception)
            {
                if (error.Length == 0)
                    error = exception.Message + ": " + exception.StackTrace;
            }

        }

        private void WinFormsHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawFrame();
        }

        #endregion

        #region WINDOW INTERACTION

        // mouse tracking
        public Point MousePosition { get; private set; } = new Point();
        private bool mouseDown = false;

        private void WinFormsHost_OnMouseMove(System.Windows.Forms.MouseEventArgs args)
        {
            //var newPosition = new Point(args.X * GetDpiScalingX(), args.Y * GetDpiScalingY());
            var newPosition = new Point(args.X, args.Y);
            
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
            imageViews[CurrentView]?.UpdateMouseDisplay(this);
        }

        private void WinFormsHost_OnMouseDown(System.Windows.Forms.MouseEventArgs args)
        {
            mouseDown = ((args.Button & MouseButtons.Left) | (args.Button & MouseButtons.Right)) != 0;
            //MousePosition = new Point(args.X * GetDpiScalingX(), args.Y * GetDpiScalingY());
            MousePosition = new Point(args.X, args.Y);
        }

        private void WinFormsHost_OnMouseUp(System.Windows.Forms.MouseEventArgs args)
        {
            mouseDown = ((args.Button & MouseButtons.Left) | (args.Button & MouseButtons.Right)) == 0;
            //MousePosition = new Point(args.X * GetDpiScalingX(), args.Y * GetDpiScalingY());
            MousePosition = new Point(args.X, args.Y);
        }

        private void WinFormsHost_OnMouseLeave(System.EventArgs args)
        {
            mouseDown = false;
        }

        private void WinFormsHost_OnMouseWheel(System.Windows.Forms.MouseEventArgs args)
        {
            imageViews[currentImageView]?.OnScroll(args.Delta, new Point(args.X, args.Y));
            imageViews[CurrentView]?.UpdateMouseDisplay(this);
            RedrawFrame();
        }
        
        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
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
                    return;
            }
            RedrawFrame();
            e.Handled = true;
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

        private void ImportImage(string filename)
        {
            try
            {
                bool resetViews = Context.GetNumImages() == 0;

                var images = ImageLoader.LoadImage(filename);
                foreach (var image in images)
                {
                    Context.AddImage(image);
                }

                if (Context.GetNumImages() > 1)
                    parent.OpenDialog(App.UniqueDialog.Image);
                

                if (resetViews)
                {
                    imageViews.Clear();
                    CreateImageViews();
                }
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

            if (ofd.ShowDialog() == true)
            {
                if (Context.GetNumImages() == 0)
                {
                    ImportImage(ofd.FileName);
                }
                else
                {
                    parent.SpawnWindow(ofd.FileName);
                }
            }
        }

        private void MenuItem_Click_Import(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = false;

            if (ofd.ShowDialog() != true) return;
            // load image and import if possible
            ImportImage(ofd.FileName);
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

        #endregion

        #region WINDOWS

        private void MenuItem_Click_Images(object sender, RoutedEventArgs e)
        {
            parent.OpenDialog(App.UniqueDialog.Image);
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

        private string RemoveFilePath(string file)
        {
            var idx = file.LastIndexOf("\\", StringComparison.Ordinal);
            if (idx > 0)
            {
                return file.Substring(idx + 1);
            }
            return file;
        }

        public ListBoxItem[] GenerateImageItems()
        {
            var items = new ListBoxItem[Context.GetNumImages()];
            for (int curImage = 0; curImage < Context.GetNumImages(); ++curImage)
            {
                items[curImage] = new ListBoxItem
                {
                    Content = $"Image {curImage} - {RemoveFilePath(Context.GetFilename(curImage))}",
                    ToolTip = Context.GetFilename(curImage)
                };
            }
            return items;
        }

        #endregion

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            parent.UnregisterWindow(this);
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            parent.SetActiveWindow(this);
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            parent.UpdateDialogVisibility();
        }

        private void MenuItem_Click_Export(object sender, RoutedEventArgs e)
        {
            if(Context.GetNumImages() == 0)
                return;

            // open save file dialog
            Microsoft.Win32.SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp|HDR (*.hdr)|*.hdr";
            if (sfd.ShowDialog() == false)
                return;

            // obtain format
            ExportWindow.FileFormat format = ExportWindow.FileFormat.Png;
            if(sfd.FileName.EndsWith(".bmp"))
                format = ExportWindow.FileFormat.Bmp;
            else if(sfd.FileName.EndsWith(".hdr"))
                format = ExportWindow.FileFormat.Hdr;

            // open dialog
            ExportWindow ew = new ExportWindow(this, sfd.FileName, format);
            if (ew.ShowDialog() == false)
                return;

            // do the export
            glControl.MakeCurrent();
            int width;
            int height;
            var data = Context.GetCurrentImageData(ew.SelectedMipmap, ew.SelectedLayer, ew.SelectedFormat,
                ew.SelectedPixelType, out width, out height);

            if (data == null)
            {
                App.ShowErrorDialog(this, "error retrieving file from gpu");
                return;
            }

            try
            {
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
                return;
            }
        }

        private void BoxScroll_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.Return)
            {
                if (imageViews.Count == 0)
                    return; // not yet initialized

                var text = BoxScroll.Text;
                if (text.EndsWith("%"))
                    text = text.Substring(0, text.Length - 1);

                decimal dec;
                if (Decimal.TryParse(text, out dec))
                {
                    imageViews[CurrentView]?.SetZoom((float)dec / 100.0f);
                    RedrawFrame();
                }
                e.Handled = true;
            }
        }

        private void MenuItem_Click_Tonemapper(object sender, RoutedEventArgs e)
        {
            parent.OpenDialog(App.UniqueDialog.Tonemap);
        }
    }
}
