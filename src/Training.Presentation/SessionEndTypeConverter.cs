using System;
using System.Globalization;
using System.Windows.Data;
using Common.Domain;

namespace Training.Presentation
{
    public class SessionEndTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = (TrainingSessionReport) value;
    
            if (t == null)
            {
                return "";
            }
    
            switch (t.SessionEndType)
            {
                case SessionEndType.TargetReached:
                    return $"Target reached ({t.Error})";
    
                case SessionEndType.Stopped:
                    return "Session stopped";
                case SessionEndType.Timeout:
                    return "Timeout";
                case SessionEndType.MaxEpoch:
                    return "Max epoch number reached";
                case SessionEndType.NaNResult:
                    return "Error reached NaN value";
                case SessionEndType.AlgorithmError:
                    return "Algorithm error";
                case SessionEndType.Paused:
                    return "Session paused";
            }
    
            return "";
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}