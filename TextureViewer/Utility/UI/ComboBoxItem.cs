using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Utility.UI
{
    public class ComboBoxItem<T>
    {
        private readonly string name;
        public T Cargo { get; }

        public ComboBoxItem(string name, T cargo)
        {
            this.name = name;
            Cargo = cargo;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
