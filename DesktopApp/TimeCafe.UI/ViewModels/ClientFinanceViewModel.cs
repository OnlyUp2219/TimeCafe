namespace TimeCafe.UI.ViewModels;

public partial class ClientFinanceViewModel : ObservableRecipient, INavigationAware
{
    private readonly IMediator _mediator;
    private int _clientId;

    [ObservableProperty] private string clientName;
    [ObservableProperty] private decimal currentBalance;
    [ObservableProperty] private decimal currentDebt;
    [ObservableProperty] private decimal depositAmount;
    [ObservableProperty] private decimal debtPaymentAmount;
    [ObservableProperty] private string depositComment;
    [ObservableProperty] private string debtPaymentComment;
    [ObservableProperty] private ObservableCollection<FinancialTransaction> transactions = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private string successMessage;
    [ObservableProperty] private bool isDepositLoading;
    [ObservableProperty] private bool isDebtPaymentLoading;

    public ClientFinanceViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void Initialize(int clientId)
    {
        _clientId = clientId;
        _ = LoadDataAsync();
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is Client client)
        {
            _clientId = client.ClientId;
            await LoadDataAsync();
        }
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
            ErrorMessage = string.Empty;

            var client = await _mediator.Send(new GetClientByIdQuery(_clientId));
            if (client != null)
            {
                ClientName = $"{client.LastName} {client.FirstName} {client.MiddleName}".Trim();
            }

            CurrentBalance = await _mediator.Send(new GetClientBalanceQuery(_clientId));
            CurrentDebt = CurrentBalance < 0 ? Math.Abs(CurrentBalance) : 0;

            var transactionsList = await _mediator.Send(new GetClientFinancialTransactionsQuery(_clientId, 50));

            Transactions.Clear();
            foreach (var transaction in transactionsList)
            {
                Transactions.Add(transaction);
            }
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ErrorMessage = $"Ошибка при загрузке данных: {msg}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DepositAsync()
    {
        if (DepositAmount <= 0)
        {
            ErrorMessage = "Сумма пополнения должна быть больше 0";
            return;
        }

        try
        {
            IsDepositLoading = true;
            ErrorMessage = string.Empty;

            await _mediator.Send(new DepositCommand(_clientId, DepositAmount, DepositComment));

            SuccessMessage = $"Баланс пополнен на {DepositAmount:C}";
            DepositAmount = 0;
            DepositComment = string.Empty;

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ErrorMessage = $"Ошибка при пополнении: {msg}";
        }
        finally
        {
            IsDepositLoading = false;
        }
    }

    [RelayCommand]
    private async Task PayDebtAsync()
    {
        if (DebtPaymentAmount <= 0)
        {
            ErrorMessage = "Сумма погашения должна быть больше 0";
            return;
        }

        if (DebtPaymentAmount > CurrentDebt)
        {
            ErrorMessage = "Сумма погашения не может превышать задолженность";
            return;
        }

        try
        {
            IsDebtPaymentLoading = true;
            ErrorMessage = string.Empty;

            await _mediator.Send(new DepositCommand(_clientId, DebtPaymentAmount, DebtPaymentComment));

            DebtPaymentAmount = 0;
            DebtPaymentComment = string.Empty;

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ErrorMessage = $"Ошибка при погашении задолженности: {msg}";
        }
        finally
        {
            IsDebtPaymentLoading = false;
        }
    }

    [RelayCommand]
    private async Task PayFullDebtAsync()
    {
        if (CurrentDebt <= 0)
        {
            ErrorMessage = "У клиента нет задолженности";
            return;
        }

        DebtPaymentAmount = CurrentDebt;
        try
        {
            await PayDebtAsync();
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ErrorMessage = $"Ошибка при полном погашении задолженности: {msg}";
        }
    }

    [RelayCommand]
    private void RefreshData()
    {
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    private void ClearData()
    {
        _clientId = 0;
        ClientName = string.Empty;
        CurrentBalance = 0;
        CurrentDebt = 0;
        DepositAmount = 0;
        DebtPaymentAmount = 0;
        DepositComment = string.Empty;
        DebtPaymentComment = string.Empty;
        Transactions.Clear();
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    partial void OnDepositAmountChanged(decimal value)
    {
        DepositAmount = Math.Round(value, 2);
    }
}