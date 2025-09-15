namespace TimeCafe.UI.Views;

public sealed partial class FinanceManagementPage : Page
{
    public FinanceManagementViewModel ViewModel { get; }

    public FinanceManagementPage()
    {
        ViewModel = App.GetService<FinanceManagementViewModel>();
        DataContext = ViewModel;
        this.InitializeComponent();
    }

    private void NavigateToClientDetail_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ClientBalanceInfo clientInfo)
        {
            ViewModel.NavigateToClientDetailCommand.Execute(clientInfo);
        }
    }

    private void ListViewContainer_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
    {
        if (ClientsListView != null)
        {
            ClientsListView.Height = e.NewSize.Height;
        }
    }
}