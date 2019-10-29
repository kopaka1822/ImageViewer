using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;

namespace ImageViewer.ViewModels.Dialog
{
    public class ScaleViewModel
    {
        private readonly ImageFramework.Model.Models models;
        public ScaleViewModel(ImageFramework.Model.Models models)
        {
            this.models = models;
            Width = models.Images.Width;
            Height = models.Images.Height;
        }

        public int Width { get; set; }

        public int Height { get; set; }

    }
}
