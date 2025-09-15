namespace TimeCafe.UI.Views;

public sealed partial class ClientFinancePage : Page
{
    public ClientFinanceViewModel ViewModel { get; }

    public ClientFinancePage()
    {
        ViewModel = App.GetService<ClientFinanceViewModel>();
        this.InitializeComponent();
    }
}