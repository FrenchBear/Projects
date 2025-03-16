// Solitaire WPF
// class MinMaxIntValidationRule
// A general validation rule with min/max attributes to be used directly inside a binding
//
// 2012-04-28   PV      First version
// 2020-12-19   PV      Net5 C#9, nullable enable
// 2021-11-13   PV      Net6 C#10
// 2025-03-16   PV      Net9 C#13

using System.Windows.Controls;

namespace Solitaire;

// Simple integer min/max validation rule
public class MinMaxIntValidationRule: ValidationRule
{
    public int Minimum { get; set; }
    public int Maximum { get; set; }
    public string Message { get; set; } = string.Empty;

    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        => value == null
            ? new ValidationResult(false, "Value is required")
            : !int.TryParse(value.ToString(), out int columns)
            ? new ValidationResult(false, "Cannot convert int value")
            : columns < Minimum || columns > Maximum ? new ValidationResult(false, Message) : ValidationResult.ValidResult;
}
