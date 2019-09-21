using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class Resource
    {
        public int Id { get; }

        public Resource(string file)
        {
            Id = Dll.open(file);
            if (Id == 0)
                throw new Exception("error in " + file + ": " + Dll.GetError());
        }

        ~Resource()
        {
            if (Id != 0)
                Dll.release(Id);
        }
    }
}
