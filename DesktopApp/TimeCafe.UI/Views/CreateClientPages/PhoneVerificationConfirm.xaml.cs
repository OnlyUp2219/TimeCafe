namespace TimeCafe.UI.Views;

public sealed partial class PhoneVerificationConfirm : Page
{
    public PhoneVerificationViewModel ViewModel
    {
        get;
    }

    public PhoneVerificationConfirm()
    {
        ViewModel = App.GetService<PhoneVerificationViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    public void SetPhoneNumber(string number)
    {
        ViewModel.SetPhoneNumber(number);
    }

    public void PrimaryButtonClick(object sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        ViewModel.ErrorMessage = string.Empty;

        var validationResult = ViewModel.ValidConfirmCode(VerificationCodeInput.Text);
        if (!string.IsNullOrEmpty(validationResult))
        {
            ViewModel.ErrorMessage = validationResult;
            args.Cancel = true;
        }
        else
        {
            args.Cancel = false;
        }

        deferral.Complete();
    }
}
