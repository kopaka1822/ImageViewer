using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Shaders;
using TextureViewer.glhelper;
using TextureViewer.ImageView;

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly App parent;
        public Context Context { get; private set; }
        public ulong ZIndex { get; set; }
        
        private ImageViewType currentImageView;
        private Dictionary<ImageViewType, IImageView> imageViews = new Dictionary<ImageViewType, IImageView>();
        public ImageViewType CurrentView
        {
            get { return currentImageView; }
            set
            {
                if (imageViews.ContainsKey(value))
                    currentImageView = value;
            }
        }

        private String errorMessage = "";


        // mouse tracking
        private Point mousePosition = new Point();

        public MainWindow(App parent, Context context)
        {
            this.parent = parent;
            this.Context = context;
            this.ZIndex = 0;

            InitializeComponent();
            
            CreateImageViews();

            context.ChangedMipmap += (sender, args) => UpdateWindowName();
            context.ChangedLayer += (sender, args) => UpdateWindowName();
            UpdateWindowName();

            if (Context.GetNumImages() == 1 && Context.GetImages()[0].IsGrayscale())
            {
                // send click event for red (grayscale with first component)
                MenuItemGrayscaleRed.IsChecked = true;
                UpdateGrayscale(MenuItemGrayscaleRed);
            }
        }

        private void CreateImageViews()
        {
            if (Context.GetImages().Count > 0)
            {
                imageViews.Add(ImageViewType.Single, new SingleView());
                currentImageView = ImageViewType.Single;
                if(Context.GetNumLayers() == 6)
                    imageViews.Add(ImageViewType.CubeMap, new CubeMapView());
            }
            else
            {
                imageViews.Add(ImageViewType.Empty, new EmptyView());
                currentImageView = ImageViewType.Empty;
            }
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

#region OpenGL

        private void OpenGLControl_OnOpenGLDraw(object sender, OpenGLEventArgs args)
        {
            if (errorMessage.Length > 0)
            {
                MessageBox.Show(errorMessage);
                errorMessage = "";
            }
            try
            {
                //  Get the OpenGL instance that's been passed to us.
                OpenGL gl = args.OpenGL;
                Utility.GlCheck(gl);

                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();

                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                gl.LoadIdentity();

                //  Clear the color and depth buffers.
                gl.ClearColor(0.9333f, 0.9333f, 0.9333f, 1.0f);
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                Utility.GlCheck(gl);

                //currentView.Draw();
                imageViews[currentImageView]?.Draw();
                Utility.GlCheck(gl);

                //  Flush OpenGL.
                gl.Flush();
            }
            catch (Exception e)
            {
                errorMessage = e.Message + "\nstack: " + e.StackTrace;
            }
           
        }

        private void OpenGLControl_OnOpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            try
            {
                // seamless cube mal
                args.OpenGL.Enable(0x884f);

                foreach(var view in imageViews)
                    view.Value.Init(args.OpenGL, this);

                Utility.GlCheck(args.OpenGL);
            }
            catch (Exception e)
            {
                errorMessage = e.Message + "\nstack: " + e.StackTrace;
            }
        }

        #endregion

#region Menu Items
        private void MenuItem_Click_Mipmaps(object sender, RoutedEventArgs e)
        {
            parent.OpenDialog(App.UniqueDialog.Mipmaps);
        }

        private void MenuItem_Click_Layers(object sender, RoutedEventArgs e)
        {
            parent.OpenDialog(App.UniqueDialog.Layer);
        }

        private void MenuItem_OnChecked_LinearFiltering(object sender, RoutedEventArgs routedEventArgs)
        {
            Context.LinearInterpolation = MenuItemLinearInterpolation.IsChecked;
        }

        private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;

            if (ofd.ShowDialog(this) == true)
            {
                if (Context.GetNumImages() == 0)
                {
                    // TODO reinit window instead of closing
                    parent.SpawnWindow(ofd.FileName);
                    this.Close();
                }
                else
                {
                    parent.SpawnWindow(ofd.FileName);
                }
            }
        }
        
        private void MenuItem_Click_Images(object sender, RoutedEventArgs e)
        {
            parent.OpenDialog(App.UniqueDialog.Image);
        }

        private void MenuItemGrayscale_OnChecked(object sender, RoutedEventArgs e)
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
                Context.Grayscale = Context.GrayscaleMode.Disabled;
            else if (MenuItemGrayscaleRed.IsChecked)
                Context.Grayscale = Context.GrayscaleMode.Red;
            else if (MenuItemGrayscaleGreen.IsChecked)
                Context.Grayscale = Context.GrayscaleMode.Green;
            else if (MenuItemGrayscaleBlue.IsChecked)
                Context.Grayscale = Context.GrayscaleMode.Blue;
            else if (MenuItemGrayscaleAlpha.IsChecked)
                Context.Grayscale = Context.GrayscaleMode.Alpha;
        }

        #endregion

#region OpenGL Control Mouse Interaction

        private bool mouseDown = false;

        private void OpenGlControl_OnMouseMove(object sender, MouseEventArgs e)
        {
            var newPosition = e.GetPosition(this.OpenGlControl);
            if (mouseDown)
            {
                // drag event
                var diff = newPosition - mousePosition;
                
                if(Math.Abs(diff.X) > 0.01 || Math.Abs(diff.Y) > 0.01)
                    imageViews[currentImageView]?.OnDrag(diff);
            }
            mousePosition = newPosition;
        }

        private void OpenGlControl_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed;
            mousePosition = e.GetPosition(this.OpenGlControl);
        }

        private void OpenGlControl_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed;
            mousePosition = e.GetPosition(this.OpenGlControl);
        }

        private void OpenGlControl_OnMouseLeave(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void OpenGlControl_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            imageViews[currentImageView]?.OnScroll((double)e.Delta, e.GetPosition(OpenGlControl));
        }

        #endregion

#region OpenGL Window Interaction
        public int GetClientWidth()
        {
            return (int)OpenGlControl.ActualWidth;
        }

        public int GetClientHeight()
        {
            return (int)OpenGlControl.ActualHeight;
        }

        private void OpenGlControl_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                
                // TODO add file to current window if definitions match
                if (files != null)
                    foreach (var file in files)
                        parent.SpawnWindow(file);
            }

        }
        #endregion

#region General Window Interactions
        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            parent.SetActiveWindow(this);
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            parent.UpdateDialogVisibility();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            parent.UnregisterWindow(this);
        }

        private void UpdateWindowName()
        {
            Title = "Texture Viewer - Layer " + Context.ActiveLayer + " - Mipmap " + Context.ActiveMipmap;
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
            }
        }
        #endregion

#region Dialog Generators

        public ListBoxItem[] GenerateImageItems()
        {
            return new ListBoxItem[0];
        }

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

#endregion


        private void MenuItem_Click_Export(object sender, RoutedEventArgs e)
        {
            // open save file dialog
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp|HDR (*.hdr)|*.hdr";
            if (sfd.ShowDialog() == false)
                return;

            ExportWindow ew = new ExportWindow(this, sfd.FileName);
            if (ew.ShowDialog() == false)
                return;

            // do the export
        }
    }
}
