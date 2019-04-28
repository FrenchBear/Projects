// Solitaire WPF
// class MinMaxIntValidationRule
// A general validation rule with min/max attributes to be used directly inside a biding
// 2012-04-28   PV  First version

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace SolWPF
{
    // Simple integer min/max validation rule
    public class MinMaxIntValidationRule : ValidationRule
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public string Message { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "Value is required");

            if (!int.TryParse(value.ToString(), out int columns))
                return new ValidationResult(false, "Cannot convert int value");

            if (columns < Minimum || columns > Maximum)
                return new ValidationResult(false, Message);

            return ValidationResult.ValidResult;
        }
    }
}
