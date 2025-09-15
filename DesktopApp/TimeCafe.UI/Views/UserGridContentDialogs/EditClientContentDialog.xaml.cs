namespace TimeCafe.UI.Views;

public sealed partial class EditClientContentDialog : Page
{
    public EditClientContentDialogViewModel ViewModel { get; }

    public EditClientContentDialog()
    {
        ViewModel = App.GetService<EditClientContentDialogViewModel>();
        DataContext = ViewModel;
        this.InitializeComponent();
    }

    public void SetData<T>(T data)
    {
        if (data is Client client)
        {
            ViewModel.SetClient(client);
        }
    }

    public void SetDialog(ContentDialog dialog)
    {
        dialog.PrimaryButtonClick += PrimaryButtonClick;
    }

    public async void PrimaryButtonClick(object sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        ViewModel.ErrorMessage = string.Empty;

        var validationResult = await ViewModel.ValidateAsync();
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