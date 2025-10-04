// UniSearchUWP
// Unicode Character Search Tool, UWP version
// Common binding converter
//
// 2018-09-18   PV
// 2020-11-11   PV      nullable enable

using System;
using Windows.UI.Xaml.Data;

#nullable enable

namespace UniSearchNS;

public class NullableBooleanToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool?)
            return (bool)value;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolean)
            return boolean;
        return false;
    }
}
