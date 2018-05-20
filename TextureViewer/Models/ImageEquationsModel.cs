using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;

namespace TextureViewer.Models
{
    /// <summary>
    /// container for the image equations
    /// </summary>
    public class ImageEquationsModel : INotifyPropertyChanged
    {
        private readonly ImageEquationModel[] equations;

        public int NumEquations => equations.Length;

        public ImageEquationsModel(ImagesModel images)
        {
            equations = new ImageEquationModel[2]
            {
                new ImageEquationModel(true, 0, images),
                new ImageEquationModel(false, 1, images)
            };
            foreach (var eq in equations)
            {
                eq.PropertyChanged += EquationOnPropertyChanged;
            }
        }

        private void EquationOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals(nameof(ImageEquationModel.Visible)))
            {
                OnPropertyChanged(nameof(NumVisible));
            }
        }

        public ImageEquationModel Get(int id)
        {
            Debug.Assert(id >= 0 && id < NumEquations);
            return equations[id];
        }

        public int NumVisible => equations.Sum(eq => eq.Visible ? 1 : 0);

        /// <summary>
        /// returns the ids of all visible equations
        /// </summary>
        /// <returns></returns>
        public List<int> GetVisibles()
        {
            var res = new List<int>();
            for (var i = 0; i < equations.Length; ++i)
            {
                if(equations[i].Visible)
                    res.Add(i);
            }

            return res;
        }

        /// <summary>
        /// returns the id of the first visible equation.
        /// Throws exception if nothing is visible
        /// </summary>
        /// <returns></returns>
        public int GetFirstVisible()
        {
            for (var i = 0; i < equations.Length; ++i)
            {
                if (equations[i].Visible)
                    return i;
            }
            throw new Exception("no image is visible");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}