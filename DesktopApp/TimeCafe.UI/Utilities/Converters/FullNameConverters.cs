using Microsoft.UI.Xaml.Data;

namespace TimeCafe.UI.Utilities.Converters;

class FullNameConverters : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Client client)
        {
            return client.FirstName + " " + client.LastName + " " + client.MiddleName;
        }
        return string.Empty;
    }
    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
