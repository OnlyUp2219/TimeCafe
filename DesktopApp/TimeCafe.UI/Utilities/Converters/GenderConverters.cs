using Microsoft.UI.Xaml.Data;

namespace TimeCafe.UI.Utilities.Converters;

class GenderConverters : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int genderId)
        {
            switch (genderId)
            {
                case 1: return "М";
                case 2: return "Ж";
                case 3: return "Н/Д";
                default: return "Н/Д";
            }
        }
        return string.Empty;
    }
    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
