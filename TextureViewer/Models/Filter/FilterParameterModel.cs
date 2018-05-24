using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Annotations;

namespace TextureViewer.Models.Filter
{
    /// <summary>
    /// filter parameter information which is dependent on a generic type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilterParameterModel<T> : FilterParameterModelBase, INotifyPropertyChanged
    {
        public class ParameterAction
        {
            private readonly T value;
            private readonly ModificationType modType;

            public ParameterAction(T value, ModificationType modType)
            {
                this.value = value;
                this.modType = modType;
            }
        }

        public T Min { get; }
        public T Max { get; }
        public T Default { get; }
        public Dictionary<Key, ParameterAction> Keybindings { get; } = new Dictionary<Key, ParameterAction>();
        public Dictionary<ActionType, ParameterAction> Actions { get; } = new Dictionary<ActionType, ParameterAction>();
        public virtual T Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FilterParameterModel(string name, int location, T min, T max, T defaultValue)
        : base(name, location)
        {
            Min = min;
            Max = max;
            Default = defaultValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
