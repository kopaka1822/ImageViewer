using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TextureViewer.ViewModels.Filter;
using Xceed.Wpf.Toolkit;

namespace TextureViewer.Views
{
    public class IntFilterParameterView : IntegerUpDown
    {
        public IntFilterParameterView(IntFilterParameterViewModel viewModel)
        {
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
        }
    }
}
