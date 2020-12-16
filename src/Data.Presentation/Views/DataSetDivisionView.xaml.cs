using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Data.Presentation.Views
{
    internal class FloatToIntConverter : IValueConverter
    {
        private bool _first = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // if (_first)
            // {
            //     _first = false;
            //     return Math.Round((decimal)(float)value, 2);
            // }
            //
            //
            // return (int) ((float)value);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // return (float)(double) value;
            return value;
        }
    }

    /// <summary>
    /// Interaction logic for DataSetDivisionView
    /// </summary>
    public partial class DataSetDivisionView : UserControl
    {
        public DataSetDivisionView()
        {
            InitializeComponent();
        }

        private void TestPercentControl_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (TestPercentControl.Value.HasValue)
            {
                TestPercentControl.Value = Math.Round(TestPercentControl.Value.Value, 2);
            }
        }

        private void ValidationPercentControl_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (ValidationPercentControl.Value.HasValue)
            {
                ValidationPercentControl.Value = Math.Round(ValidationPercentControl.Value.Value, 2);
            }
        }

        private void TrainingPercentControl_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (TrainingPercentControl.Value.HasValue)
            {
                TrainingPercentControl.Value = Math.Round(TrainingPercentControl.Value.Value, 2);
            }
        }
    }
}
