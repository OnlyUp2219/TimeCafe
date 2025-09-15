namespace TimeCafe.UI.ViewModels;

public partial class EditClientContentDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string firstName;

    [ObservableProperty]
    private string lastName;

    [ObservableProperty]
    private string middleName;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string phoneNumber;

    [ObservableProperty]
    private DateOnly? birthDate;

    [ObservableProperty]
    private int? genderId;

    [ObservableProperty]
    private ObservableCollection<Gender> genders = new();

    [ObservableProperty]
    private string errorMessage;

    private Client _client;
    private readonly IMediator _mediator;
    private readonly IClientValidation _clientValidation;
    private string _originalPhoneNumber;

    public EditClientContentDialogViewModel(IMediator mediator, IClientValidation clientValidation)
    {
        _mediator = mediator;
        _clientValidation = clientValidation;
        LoadGendersAsync();
    }

    private async void LoadGendersAsync()
    {
        var genders = await _mediator.Send(new GetGendersQuery());
        Genders.Clear();
        foreach (var gender in genders)
        {
            Genders.Add(gender);
        }
    }

    public void SetClient(Client client)
    {
        _client = client;
        FirstName = client.FirstName;
        LastName = client.LastName;
        MiddleName = client.MiddleName;
        Email = client.Email;
        PhoneNumber = client.PhoneNumber;
        _originalPhoneNumber = client.PhoneNumber;
        BirthDate = client.BirthDate;
        GenderId = client.GenderId;
    }

    public bool IsPhoneNumberChanged()
    {
        return PhoneNumber != _originalPhoneNumber;
    }

    public async Task<string> ValidateAsync()
    {
        var sb = new StringBuilder();

        if (string.IsNullOrWhiteSpace(FirstName))
            sb.AppendLine("Имя обязательно для заполнения");

        if (!string.IsNullOrWhiteSpace(Email))
        {
            var validMail = await _clientValidation.ValidateEmailAsync(Email);
            if (validMail)
                sb.AppendLine("Неверный формат email");
        }

        if (!string.IsNullOrWhiteSpace(PhoneNumber))
        {
            if (IsPhoneNumberChanged())
            {
                var validPhone = await _clientValidation.ValidatePhoneNumberAsync(PhoneNumber);
                if (!validPhone)
                    sb.AppendLine("Неверный формат номера телефона или такой номер уже существует");
            }
            else
            {
                var validPhoneFormat = await _clientValidation.ValidatePhoneNumberFormatAsync(PhoneNumber);
                if (!validPhoneFormat)
                    sb.AppendLine("Неверный формат номера телефона");
            }
        }

        return sb.ToString();
    }

    public Client GetUpdatedClient()
    {
        if (_client == null)
            return null;

        _client.FirstName = FirstName;
        _client.LastName = LastName;
        _client.MiddleName = MiddleName;
        _client.Email = Email;
        _client.PhoneNumber = PhoneNumber;
        _client.BirthDate = BirthDate;
        _client.GenderId = GenderId;

        return _client;
    }

}