// UniSearchUWP
// Unicode Character Search Tool, UWP version
// Common binding converter
//
// 2018-09-18   PV


using System;
using Windows.UI.Xaml.Data;


namespace UniSearchUWPNS
{
    public class NullableBooleanToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool?)
            {
                return (bool)value;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return (bool)value;
            return false;
        }
    }
}
