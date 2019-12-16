using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ImageViewer.Views.Converter
{
    public class BoolVisibilityConverter : IValueConverter
    {
        private readonly Visibility trueValue;
        private readonly Visibility falseValue;

        public BoolVisibilityConverter(Visibility trueValue, Visibility falseValue)
        {
            this.trueValue = trueValue;
            this.falseValue = falseValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            Debug.Assert(targetType == typeof(Visibility));
            var b = (bool) value;
            return b ? trueValue : falseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            Debug.Assert(targetType == typeof(bool));
            var v = (Visibility) value;
            if (v == trueValue) return true;
            Debug.Assert(v == falseValue);
            return false;
        }
    }

    // create overloads:

    // true => visible, false => collapsed
    public class VisibleCollapsedConverter : BoolVisibilityConverter
    {
        public VisibleCollapsedConverter() : base(Visibility.Visible, Visibility.Collapsed)
        {}
    }

    // true => collapsed, false => visible
    public class CollapsedVisibleConverter : BoolVisibilityConverter
    {
        public CollapsedVisibleConverter() : base(Visibility.Collapsed, Visibility.Visible)
        {}
    }

    // true => visible, false => hidden
    public class VisibleHiddenConverter : BoolVisibilityConverter
    {
        public VisibleHiddenConverter() : base(Visibility.Visible, Visibility.Hidden)
        { }
    }

    // true => hidden, false => visible
    public class HiddenVisibleConverter : BoolVisibilityConverter
    {
        public HiddenVisibleConverter() : base(Visibility.Hidden, Visibility.Visible)
        { }
    }
}
