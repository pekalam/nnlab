using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SharedUI
{
    public class DoubleValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return double.TryParse((string)value, NumberStyles.Float, CultureInfo.CurrentCulture, out _)
                ? System.Windows.Controls.ValidationResult.ValidResult
                : new System.Windows.Controls.ValidationResult(false, "Invalid value");
        }
    }
}