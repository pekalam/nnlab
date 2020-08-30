using System;
using System.Globalization;
using System.Windows.Data;
using Common.Domain;

namespace Training.Presentation.Converters
{
    public class TrainingAlgorithmTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = (TrainingParameters)value;

            if (param == null)
            {
                return "";
            }

            switch (param.Algorithm)
            {
                case TrainingAlgorithm.GradientDescent:
                    return "Gradient descent" + (param.GDParams.BatchSize == 1 ? " (online)" : "");
                case TrainingAlgorithm.LevenbergMarquardt:
                    return "Levenberg-Marquardt";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}