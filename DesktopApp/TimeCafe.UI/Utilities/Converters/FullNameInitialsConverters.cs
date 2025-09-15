using Microsoft.UI.Xaml.Data;

namespace TimeCafe.UI.Utilities.Converters;

class FullNameInitialsConverters : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Client client)
            return string.Empty;

        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(client.FirstName))
        {
            sb.Append(client.FirstName);
        }

        if (!string.IsNullOrWhiteSpace(client.LastName))
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(client.LastName[0] + ".");
        }

        if (!string.IsNullOrWhiteSpace(client.MiddleName))
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(client.MiddleName[0] + ".");
        }

        return sb.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
