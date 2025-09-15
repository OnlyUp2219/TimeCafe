using Microsoft.UI.Xaml.Data;

namespace TimeCafe.UI.Utilities.Converters;

public class DateOnlyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateOnly dateOnly)
        {
            var dateTime = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);
            return new DateTimeOffset(dateTime);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            var dateTime = dateTimeOffset.DateTime;
            return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
        }
        return null;
    }
}