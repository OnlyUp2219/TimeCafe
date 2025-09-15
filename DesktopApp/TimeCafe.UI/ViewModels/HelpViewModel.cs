namespace TimeCafe.UI.ViewModels;

public partial class HelpViewModel : ObservableRecipient, INavigationAware
{
    public HelpViewModel() { }


    public void OnNavigatedTo(object parameter)
    {

    }

    public void OnNavigatedFrom()
    {
        // Логика при уходе со страницы справки (если потребуется)
    }
}