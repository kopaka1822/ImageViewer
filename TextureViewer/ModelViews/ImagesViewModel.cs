using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Models;

namespace TextureViewer.ModelViews
{
    public class ImagesViewModel
    {
        public Images Images { get; }

        public ImagesViewModel(Images images)
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
