using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Equation
{
    class ImageToken : Token
    {
        private int id;

        public ImageToken(int id) : 
            base(Type.Value)
        {
            this.id = id;
        }
    }
}
