using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using TextureViewer.Utility;

namespace TextureViewer.Models
{
    public class ImageEquationModel : INotifyPropertyChanged
    {
        private ImagesModel images;

        public ImageEquationModel(bool visible, int defaultImage, ImagesModel images)
        {
            this.images = images;
            this.visible = visible;
            ColorFormula = new FormulaModel(defaultImage, images, this);
            AlphaFormula = new FormulaModel(defaultImage, images, this);
        }

        private bool visible;
        public bool Visible
        {
            get => visible;
            set
            {
                if (value == visible) return;
                visible = value;
                OnPropertyChanged(nameof(Visible));
            }
        }

        private bool useFilter = true;
        public bool UseFilter
        {
            get => useFilter;
            set
            {
                if (value == useFilter) return;
                useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
            }
        }

        public FormulaModel ColorFormula { get; }
        public FormulaModel AlphaFormula { get; }

        private Color texelColor = Color.ZERO;
        public Color TexelColor
        {
            get => texelColor;
            set
            {
                if (texelColor.Equals(value)) return;
                texelColor = value;
                OnPropertyChanged(nameof(TexelColor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
