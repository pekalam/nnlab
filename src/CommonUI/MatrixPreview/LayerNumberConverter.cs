using System;
using System.Globalization;
using System.Windows.Data;

namespace CommonUI.MatrixPreview
{
    public class LayerNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ind = System.Convert.ToInt32(value);

            return $"Layer {ind}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}