namespace TimeCafe.UI.ViewModels;

public partial class RefuseServiceContentDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string reason;

    [ObservableProperty]
    private string errorMessage;

    private Client _client;

    public void SetClient(Client client)
    {
        _client = client;
        Reason = string.Empty;
        ErrorMessage = string.Empty;
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Reason))
        {
            ErrorMessage = "Причина отказа обязательна для заполнения";
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}