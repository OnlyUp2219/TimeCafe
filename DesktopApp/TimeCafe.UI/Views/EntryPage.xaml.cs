namespace TimeCafe.UI.Views;

public sealed partial class EntryPage : Page
{
    public EntryViewModel ViewModel
    {
        get;
    }

    public EntryPage()
    {
        ViewModel = App.GetService<EntryViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    public void ShowError(string message)
    {
        ErrorTeachingTip.Subtitle = message;
        ErrorTeachingTip.IsOpen = true;
    }

    // private void TariffsItemsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    // {
    //     if (ViewModel != null && sender is ListView listView)
    //     {
    //         ViewModel.SelectedTariff = listView.SelectedItem as Tariff;
    //     }
    // }
}