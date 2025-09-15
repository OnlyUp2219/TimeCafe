using Microsoft.UI.Xaml.Data;

namespace TimeCafe.UI.Utilities.Converters;

public class StatusToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int statusId && parameter is string targetStatus)
        {
            return targetStatus switch
            {
                "Draft" => statusId == (int)EClientStatusType.Draft ? Visibility.Visible : Visibility.Collapsed,
                "Active" => statusId == (int)EClientStatusType.Active ? Visibility.Visible : Visibility.Collapsed,
                "Rejected" => statusId == (int)EClientStatusType.Rejected ? Visibility.Visible : Visibility.Collapsed,
                "NotDraft" => statusId != (int)EClientStatusType.Draft ? Visibility.Visible : Visibility.Collapsed,
                "NotActive" => statusId != (int)EClientStatusType.Active ? Visibility.Visible : Visibility.Collapsed,
                "NotRejected" => statusId != (int)EClientStatusType.Rejected ? Visibility.Visible : Visibility.Collapsed,
                _ => Visibility.Collapsed
            };
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}