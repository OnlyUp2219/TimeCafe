namespace TimeCafe.UI.Views;

public sealed partial class VisitorManagementPage : Page
{
    public VisitorManagementViewModel ViewModel
    {
        get;
    }

    public VisitorManagementPage()
    {
        ViewModel = App.GetService<VisitorManagementViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    private async void ExitButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Visit visit)
        {
            await ViewModel.ExitVisitorAsync(visit);
        }
    }
}