using CommunityToolkit.WinUI.UI.Controls;



namespace TimeCafe.UI.ViewModels;

public partial class CreateClientViewModel : ObservableRecipient, INavigationAware
{
    private bool _isGridViewSelected = true;
    private static int _currentPage = 1;
    private const int PageSize = 16;

    [ObservableProperty] private int totalItems;
    [ObservableProperty] private bool isLoading;

    public bool IsGridViewSelected
    {
        get => _isGridViewSelected;
        set
        {
            if (_isGridViewSelected != value)
            {
                _isGridViewSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActiveView));

                if (AdaptiveGrid != null && ListView != null)
                {
                    if (_isGridViewSelected)
                    {
                        var selectedItems = ListView.SelectedItems.ToList();
                        AdaptiveGrid.SelectedItems.Clear();
                        foreach (var item in selectedItems)
                        {
                            AdaptiveGrid.SelectedItems.Add(item);
                        }
                    }
                    else
                    {
                        var selectedItems = AdaptiveGrid.SelectedItems.ToList();
                        ListView.SelectedItems.Clear();
                        foreach (var item in selectedItems)
                        {
                            ListView.SelectedItems.Add(item);
                        }
                    }
                }
            }
        }
    }

    public ListView ListView { get; set; }
    public AdaptiveGridView AdaptiveGrid { get; set; }

    public ListViewBase ActiveView => IsGridViewSelected ? AdaptiveGrid : ListView;

    [ObservableProperty] private ObservableCollection<Client> source = new();
    [ObservableProperty] private string firstName;
    [ObservableProperty] private string lastName;
    [ObservableProperty] private string middleName;
    [ObservableProperty] private int? genderId;
    [ObservableProperty] private string email;
    [ObservableProperty] private DateOnly? birthDate = DateOnly.FromDateTime(DateTime.Now);
    [ObservableProperty] private string phoneNumber;
    [ObservableProperty] private string additionalInfo;
    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private ObservableCollection<Gender> genders = new();
    [ObservableProperty] private ObservableCollection<ClientStatus> clientStatuses = new();

    private readonly IMediator _mediator;
    private readonly IClientValidation _clientValidation;
    private readonly INavigationService _navigationService;
    private readonly BogusDataGeneratorServices _fakeDataGenerator;

    public CreateClientViewModel(IMediator mediator,
        IClientValidation clientValidation,
        INavigationService navigationService,
        BogusDataGeneratorServices fakeDataGenerator)
    {
        _mediator = mediator;
        _clientValidation = clientValidation;
        _navigationService = navigationService;
        _fakeDataGenerator = fakeDataGenerator;
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
        ClearData();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            Source.Clear();
            Genders.Clear();
            ClientStatuses.Clear();

            var genders = await _mediator.Send(new GetGendersQuery());
            foreach (var gender in genders)
            {
                Genders.Add(gender);
            }

            var statuses = await _mediator.Send(new GetClientStatusesQuery());
            foreach (var status in statuses)
            {
                ClientStatuses.Add(status);
            }

            var items = await _mediator.Send(new GetClientsPageQuery(_currentPage, PageSize));
            var total = await _mediator.Send(new GetTotalPageTariffQuery());
            TotalItems = total;

            foreach (var client in items)
            {
                Source.Add(client);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearData()
    {
        Source.Clear();
        Genders.Clear();
        ClientStatuses.Clear();
        TotalItems = 0;

        FirstName = string.Empty;
        LastName = string.Empty;
        MiddleName = string.Empty;
        GenderId = null;
        Email = string.Empty;
        BirthDate = null;
        PhoneNumber = string.Empty;
        AdditionalInfo = string.Empty;
        ErrorMessage = string.Empty;
    }

    public async Task<string> ValidateAsync()
    {
        var sb = new StringBuilder();

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            sb.AppendLine("Имя и фамилия обязательны для заполнения");

        if (!string.IsNullOrWhiteSpace(Email))
        {
            var validMail = await _clientValidation.ValidateEmailAsync(Email);
            if (!validMail)
                sb.AppendLine("Неверный формат email");
        }

        var validPhone = await _clientValidation.ValidatePhoneNumberAsync(PhoneNumber);
        if (!validPhone)
            sb.AppendLine("Номер телефона не валиден");


        return sb.ToString();
    }

    [RelayCommand]
    private async Task CreateClientAsync()
    {
        var validationResult = await ValidateAsync();
        if (!string.IsNullOrEmpty(validationResult))
        {
            ErrorMessage = validationResult;
            return;
        }

        var dialog = PhoneVerificationDialogFactory.Create(
            PhoneNumber,
            App.MainWindow.Content.XamlRoot,
            "Подтверждение телефона",
            "Подтвердить",
            "Пропустить",
            "Отменить"

        );

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await CreateClientAsync(true);
        }
        else if (result == ContentDialogResult.Secondary)
        {
            await CreateClientAsync(false);
        }
    }

    public async Task CreateClientAsync(bool isActive)
    {
        if (await ValidateAsync() == string.Empty)
        {
            var client = new Client
            {
                FirstName = FirstName,
                LastName = LastName,
                MiddleName = MiddleName,
                GenderId = GenderId,
                Email = Email,
                BirthDate = BirthDate,
                PhoneNumber = PhoneNumber,
                StatusId = (int)EClientStatusType.Draft,
                CreatedAt = DateTime.Now
            };

            if (isActive)
            {
                client.StatusId = (int)EClientStatusType.Active;
            }

            if (!string.IsNullOrWhiteSpace(AdditionalInfo))
            {
                client.ClientAdditionalInfos.Add(new ClientAdditionalInfo
                {
                    InfoText = AdditionalInfo,
                    CreatedAt = DateTime.Now
                });
            }

            try
            {

                await _mediator.Send(new CreateClientCommand(client));
                ClearData();
                await LoadDataAsync();
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при создании клиента: {ex.Message}";
            }
        }
        else
        {
            ErrorMessage = await ValidateAsync();
        }
    }

    [RelayCommand]
    private void GenerateFakeData()
    {
        var fakeClient = _fakeDataGenerator.GenerateClient();

        FirstName = fakeClient.FirstName;
        LastName = fakeClient.LastName;
        MiddleName = fakeClient.MiddleName;
        GenderId = fakeClient.GenderId;
        Email = fakeClient.Email;
        BirthDate = fakeClient.BirthDate;
        PhoneNumber = fakeClient.PhoneNumber;
    }

    public async Task SetCurrentPage(int page)
    {
        if (_currentPage != page)
        {
            try
            {
                IsLoading = true;
                _currentPage = page;
                Source.Clear();

                var items = await _mediator.Send(new GetClientsPageQuery(_currentPage, PageSize));
                var total = await _mediator.Send(new GetTotalPageClientQuery());
                TotalItems = total;

                foreach (var client in items)
                {
                    Source.Add(client);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
