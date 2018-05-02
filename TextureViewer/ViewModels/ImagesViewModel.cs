using System;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class ImagesViewModel
    {
        public ImagesModel Images { get; }

        public ImagesViewModel(ImagesModel images)
        {
            this.Images = images;
        }

        /// <summary>
        /// opens a file dialoge to import images
        /// </summary>
        public void ImportImage()
        {
            // TODO add multi select
            var ofd = new Microsoft.Win32.OpenFileDialog {Multiselect = false};
            // TODO set initial directory

            if (ofd.ShowDialog() != true) return;

            // TODO set new inital directory
            // load image
            try
            {
                var imgs = ImageLoader.LoadImage(ofd.FileName);
                Images.AddImages(imgs);
            }
            catch (Exception e)
            {
                // TODO put window reference here
                App.ShowErrorDialog(null, e.Message);
            }
        }
    }
}
