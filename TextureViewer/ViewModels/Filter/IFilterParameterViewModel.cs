using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.ViewModels.Filter
{
    public interface IFilterParameterViewModel
    {
        /// <summary>
        /// applies the result from the num box to the model
        /// </summary>
        void Apply();
    }
}
