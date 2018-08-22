using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;

namespace TextureViewer.Models.Filter
{
    public class FilterTextureParameterModel : INotifyPropertyChanged
    {
        public string Name { get; }
        public int Binding { get; }
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

        public FilterTextureParameterModel(string name, int binding)
        {
            Name = name;
            Binding = binding;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
