using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;

namespace ImageFramework.Model.Filter.Parameter
{
    public class TextureFilterParameterModel : INotifyPropertyChanged
    {
        /// display name
        public string Name { get; }
        /// shader name
        public string TextureName { get; }


        private int source = 0;

        // describes which source images is used for the binding
        public int Source
        {
            get => source;
            set
            {
                if (value == source) return;
                source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        public TextureFilterParameterModel(string name, string textureName)
        {
            Name = name;
            TextureName = textureName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
