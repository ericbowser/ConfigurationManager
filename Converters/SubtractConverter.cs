using System;
using System.Globalization;
using System.Windows.Data;

namespace ConfigurationManager.Converters
{
    /// <summary>
    /// Value converter that subtracts a parameter value from the input value.
    /// Useful for dynamic width/height calculations in WPF layouts.
    /// </summary>
    public class SubtractConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string paramString)
            {
                if (double.TryParse(paramString, out double subtract))
                {
                    return Math.Max(0, doubleValue - subtract);
                }
            }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("SubtractConverter does not support ConvertBack");
        }
    }
}
