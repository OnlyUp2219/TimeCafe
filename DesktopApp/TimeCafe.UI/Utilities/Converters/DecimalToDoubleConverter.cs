using Microsoft.UI.Xaml.Data;

namespace TimeCafe.UI.Utilities.Converters;

public class DecimalToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal decimalValue)
        {
            return (double)decimalValue;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value == null)
            return 0m;
        if (value is double doubleValue)
        {
            if (double.IsNaN(doubleValue))
                return 0m;
            return (decimal)doubleValue;
        }
        if (value is string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return 0m;
            if (str.Trim().ToLower() == "nan")
                return 0m;
        }
        return 0m;
    }
}