using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TextureViewer.Models.Filter;
using TextureViewer.ViewModels.Filter;
using Xceed.Wpf.Toolkit;

namespace TextureViewer.Views
{
    public class IntFilterParameterView : IntegerUpDown
    {
        private IntFilterParameterViewModel viewModel;

        public IntFilterParameterView(IntFilterParameterViewModel viewModel, Binding enabledBinding)
        {
            this.viewModel = viewModel;

            Margin = new Thickness(0.0, 0.0, 0.0, 2.0);
            CultureInfo = App.GetCulture();
            Increment = 0;
            DataContext = viewModel;

            // TODO custom handler for up down buttons
            // default value change handler
            var valueBinding = new Binding
            {
                Source = viewModel,
                Path = new PropertyPath("Value"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
            };
            BindingOperations.SetBinding(this, ValueProperty, valueBinding);
            BindingOperations.SetBinding(this, IsEnabledProperty, enabledBinding);

            KeyUp += OnKeyUp;

            Spinned += OnSpinned;
        }

        private void OnSpinned(object sender, SpinEventArgs args)
        {
            if (args.Direction == SpinDirection.Increase)
            {
                viewModel.InvokeAction(ActionType.OnAdd);
            }
            else if (args.Direction == SpinDirection.Decrease)
            {
                viewModel.InvokeAction(ActionType.OnSubtract);
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // update property on enter
            if (e.Key != Key.Enter)
            {
                base.OnKeyUp(e);
                e.Handled = true;
                return;
            }

            e.Handled = true;
            var binding = BindingOperations.GetBindingExpression(this, ValueProperty);
            binding?.UpdateSource();
            Keyboard.ClearFocus();
        }
    }
}
