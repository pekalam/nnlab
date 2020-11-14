using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Common.Presentation.ValidationRules
{
    public class FloatValidationRule : ValidationRule
    {
        private static Regex _regex = new Regex(@"^[0-9]*\.?[0-9]+$", RegexOptions.None);

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return _regex.Match(value.ToString()).Length > 0
                ? System.Windows.Controls.ValidationResult.ValidResult
                : new System.Windows.Controls.ValidationResult(false, "Invalid value");
        }
    }
}