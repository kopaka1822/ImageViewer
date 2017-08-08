using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer
{
    interface IUniqueDialog
    {
        bool IsClosing { get; set; }

        /// <summary>
        /// Updates content of this window depending on the passed window
        /// </summary>
        /// <param name="window"></param>
        void UpdateContent(MainWindow window);
    }
}
