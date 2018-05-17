using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models
{
    public class FinalImagesModel
    {
        private readonly FinalImageModel[] images;

        public int NumImages => images.Length;

        public FinalImagesModel(TextureCacheModel cache, ImagesModel images)
        {
            this.images = new FinalImageModel[2]
            {
                new FinalImageModel(cache, images),
                new FinalImageModel(cache, images)
            };
        }

        public FinalImageModel Get(int id)
        {
            Debug.Assert(id >= 0 && id < NumImages);
            return images[id];
        }

        public void Dispose()
        {
            foreach (var img in images)
            {
                img.Dispose();
            }
        }
    }
}
