using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;

namespace ImageFramework.ImageLoader
{
    public class Resource : IDisposable
    {
        public int Id { get; private set; }

        public Resource(string file)
        {
            Id = Dll.image_open(file);
            if (Id == 0)
                throw new Exception("error in " + file + ": " + Dll.GetError());
        }

        private Resource()
        {
            Id = 0;
        }

        public static Resource CreateWhiteNoise(Size3 size, LayerMipmapCount lm, int seed)
        {
            var res = new Resource();
            res.Id = Dll.noise_generate_white(size.Width, size.Height, size.Depth, lm.Layers, lm.Mipmaps, seed);
            if (res.Id == 0)
                throw new Exception("error generating white noise: " + Dll.GetError());
            return res;
        }

        public static Resource CreateBlueNoise(Size3 size, LayerMipmapCount lm)
        {
            var res = new Resource();
            res.Id = Dll.noise_generate_blue(size.Width, size.Height, size.Depth, lm.Layers, lm.Mipmaps);
            if (res.Id == 0)
                throw new Exception("error generating blue noise: " + Dll.GetError());
            return res;
        }

        public Resource(uint format, Size3 size, LayerMipmapCount lm)
        {
            Id = Dll.image_allocate(format, size.Width, size.Height, size.Depth, lm.Layers, lm.Mipmaps);
            if(Id == 0)
                throw new Exception("error allocating image: " + Dll.GetError());
        }

        ~Resource()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Id != 0)
            {
                Dll.image_release(Id);
                Id = 0;
            }
        }
    }
}
