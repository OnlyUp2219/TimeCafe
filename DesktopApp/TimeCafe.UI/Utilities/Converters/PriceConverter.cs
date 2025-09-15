using Microsoft.UI.Xaml.Data;

namespace TimeCafe.UI.Utilities.Converters;

public class PriceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal price)
        {
            int decimals = 2;
            if (parameter != null && int.TryParse(parameter.ToString(), out int paramDecimals))
            {
                decimals = paramDecimals;
            }

            return $"{Math.Round(price, decimals)} ₽";
        }
        return "0 ₽";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}