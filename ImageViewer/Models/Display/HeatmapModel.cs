using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Models.Display
{
    public class HeatmapModel
    {
        private readonly ImageFramework.Model.Models models;

        public HeatmapModel(ImageFramework.Model.Models models)
        {
            this.models = models;
        }
    }
}
