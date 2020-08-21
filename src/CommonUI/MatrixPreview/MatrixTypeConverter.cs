using System;
using System.Globalization;
using System.Windows.Data;

namespace SharedUI.MatrixPreview
{
    public class MatrixTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter) ?? Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool) value) ? parameter : Binding.DoNothing;
        }
    }
}