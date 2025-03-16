using System;
using System.Globalization;
using System.Windows.Data;

namespace QwirkleUI;

public class ParamToBooleanConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        // Forcing ToString enables the use of the same converter for ints and enums
        (value?.ToString() ?? "").Equals(parameter.ToString());

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value.Equals(true) ? parameter : Binding.DoNothing;
}