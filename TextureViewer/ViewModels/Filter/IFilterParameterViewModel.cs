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

        /// <summary>
        /// restores the values from the model
        /// </summary>
        void Cancel();

        /// <summary>
        /// restores default values from the model
        /// </summary>
        void RestoreDefaults();

        /// <summary>
        /// indicates if a parameter was changed
        /// </summary>
        /// <returns></returns>
        bool HasChanges();

        event EventHandler Changed;
    }
}
