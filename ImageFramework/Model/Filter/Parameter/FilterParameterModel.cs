using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageFramework.Annotations;

namespace ImageFramework.Model.Filter.Parameter
{
    /// <summary>
    /// filter parameter information which is dependent on a generic type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FilterParameterModel<T> : FilterParameterModelBase, INotifyPropertyChanged
    {
        public abstract class ParameterAction
        {
            protected readonly T OpValue;
            protected readonly ModificationType ModType;

            protected ParameterAction(T value, ModificationType modType)
            {
                this.OpValue = value;
                this.ModType = modType;
            }

            /// <summary>
            /// applies the operation on the given value and returns the modified value
            /// </summary>
            /// <param name="value">value after action</param>
            /// <returns></returns>
            public abstract T Invoke(T value);
        }

        public T Min { get; }
        public T Max { get; }
        public T Default { get; }
        public Dictionary<Key, ParameterAction> Keybindings { get; } = new Dictionary<Key, ParameterAction>();
        public Dictionary<ActionType, ParameterAction> Actions { get; } = new Dictionary<ActionType, ParameterAction>();

        public abstract T Value { get; set; }

        public FilterParameterModel(string name, string variableName, T min, T max, T defaultValue)
            : base(name, variableName)
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