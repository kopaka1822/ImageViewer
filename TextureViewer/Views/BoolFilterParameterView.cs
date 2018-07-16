using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TextureViewer.ViewModels.Filter;

namespace TextureViewer.Views
{
    public class BoolFilterParameterView : CheckBox
    {
        public BoolFilterParameterView(BoolFilterParameterViewModel viewModel)
        {
            Margin = new Thickness(0.0, 0.0, 0.0, 2.0);
            DataContext = viewModel;

            // default value change handler
            var valueBinding = new Binding
            {
                Source = viewModel,
                Path = new PropertyPath("Value"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
            };
            BindingOperations.SetBinding(this, IsCheckedProperty, valueBinding);

            
        }
    }
}
