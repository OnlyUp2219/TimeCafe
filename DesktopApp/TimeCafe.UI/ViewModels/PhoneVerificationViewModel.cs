namespace TimeCafe.UI.ViewModels;

public partial class PhoneVerificationViewModel : ObservableRecipient
{
    [ObservableProperty] private string phoneNumber;
    [ObservableProperty] private bool isPhoneVerified;
    [ObservableProperty] private string verificationCode;
    [ObservableProperty] private string errorMessage;

    public PhoneVerificationViewModel()
    {
    }

    public void SetPhoneNumber(string number)
    {
        PhoneNumber = number;
        IsPhoneVerified = false;
        VerificationCode = string.Empty;
        ErrorMessage = string.Empty;
    }

    public string ValidConfirmCode(string code)
    {
        var sb = new StringBuilder();
        if (string.IsNullOrWhiteSpace(code))
            return "Код не может быть пустым.";
        if (code != "12345")
            return "Неверный код подтверждения.";
        return sb.ToString();
    }
}

