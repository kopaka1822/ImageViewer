using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Models;

namespace TextureViewer.Controller
{
    public class ViewModeController
    {
        private DisplayModel display;

        public ViewModeController(DisplayModel display)
        {
            this.display = display;
            display.PropertyChanged += ViewModeModelOnPropertyChanged;
        }

        private void ViewModeModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.ActiveView):

                    break;
            }
        }
    }
}
