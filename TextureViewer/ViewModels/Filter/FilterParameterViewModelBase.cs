using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Annotations;
using TextureViewer.Models.Filter;

namespace TextureViewer.ViewModels.Filter
{
    public class FilterParameterViewModelBase<T> : IFilterParameterViewModel, INotifyPropertyChanged
    {
        private readonly FilterParameterModel<T> parameter;

        public FilterParameterViewModelBase(FilterParameterModel<T> parameter)
        {
            this.parameter = parameter;
            parameter.PropertyChanged += ParameterOnPropertyChanged;
        }

        private void ParameterOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(FilterParameterModel<T>.Value))
            {
                Value = parameter.Value;
            }
        }

        public virtual T Value { get=>throw new NotImplementedException(); set => throw new NotImplementedException();}


        public void Apply()
        {
            parameter.Value = Value;
        }

        public void Cancel()
        {
            Value = parameter.Value;
        }

        public void RestoreDefaults()
        {
            Value = parameter.Default;
        }

        public bool HasChanges()
        {
            return !Equals(Value, parameter.Value);
        }

        public void InvokeAction(ActionType action)
        {
            if (!parameter.Actions.TryGetValue(action, out var pa)) return;

            // invoke action on local parameter
            Value = pa.Invoke(Value);
        }

        public event EventHandler Changed;
        public bool HasKeyToInvoke(Key key)
        {
            return parameter.Keybindings.ContainsKey(key);
        }

        public void InvokeKey(Key key)
        {
            if(!parameter.Keybindings.TryGetValue(key, out var pa)) return;

            // invole on local parameter
            Value = pa.Invoke(Value);
        }

        public void Dispose()
        {
            parameter.PropertyChanged -= ParameterOnPropertyChanged;
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
