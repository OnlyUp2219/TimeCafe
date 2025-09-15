namespace TimeCafe.UI.Views;

public sealed partial class HelpPage : Page
{
    public HelpViewModel ViewModel { get; }

    public HelpPage()
    {
        ViewModel = App.GetService<HelpViewModel>();
        this.InitializeComponent();
        LoadDocumentationAsync();
    }

    private async void LoadDocumentationAsync()
    {
        try
        {
            var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Documentation/index.html"));
            MyWebView2.Source = new Uri(storageFile.Path);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки документации: {ex.Message}");
        }
    }

}