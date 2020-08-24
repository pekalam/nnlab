using System;
using System.Globalization;
using System.Windows.Data;

namespace Training.Presentation.Converters
{
    public class FloatingPointTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (double) value;
            return v.ToString("###0.###################");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
