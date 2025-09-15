using Microsoft.UI.Xaml.Data;

namespace TimeCafe.UI.Utilities.Converters;

public class TooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var client = value as Client;
        if (client != null)
        {
            return $"ФИО: {client.LastName ?? ""} {client.FirstName ?? ""} {client.MiddleName ?? ""}\nТелефон: {client.PhoneNumber ?? ""}\nСтатус: {client.Status?.StatusName ?? ""}";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null!;
    }

}
