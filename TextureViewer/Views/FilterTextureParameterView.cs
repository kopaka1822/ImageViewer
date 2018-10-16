using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using TextureViewer.ViewModels.Filter;

namespace TextureViewer.Views
{
    public class FilterTextureParameterView : ComboBox 
    {
        public FilterTextureParameterView(FilterTextureParameterViewModel viewModel, Binding enabledBinding)
        {
            Margin = new System.Windows.Thickness(0.0, 0.0, 0.0, 2.0);
            DataContext = viewModel;

            var selectedBinding = new Binding
            {
                Source = viewModel,
                Path = new System.Windows.PropertyPath("SelectedTexture"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            var listBinding = new Binding
            {
                Source = viewModel,
                Path = new System.Windows.PropertyPath("AvailableTextures"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(this, IsEnabledProperty, enabledBinding);
            BindingOperations.SetBinding(this, SelectedItemProperty, selectedBinding);
            BindingOperations.SetBinding(this, ItemsSourceProperty, listBinding);
        }
    }
}
