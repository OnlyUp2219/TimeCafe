namespace TimeCafe.UI.Views;

public sealed partial class RefuseServiceContentDialog : Page
{
    public RefuseServiceContentDialogViewModel ViewModel { get; }

    public RefuseServiceContentDialog()
    {
        ViewModel = App.GetService<RefuseServiceContentDialogViewModel>();
        DataContext = ViewModel;
        this.InitializeComponent();
    }

    public void PrimaryButtonClick(object sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        ViewModel.ErrorMessage = string.Empty;

        if (!ViewModel.Validate())
        {
            args.Cancel = true;
        }

        deferral.Complete();
    }
}