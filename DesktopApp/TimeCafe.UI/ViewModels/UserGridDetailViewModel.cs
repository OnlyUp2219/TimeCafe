using TimeCafe.UI.Views.UserGridContentDialogs;

namespace TimeCafe.UI.ViewModels;

public partial class UserGridDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Client? _item;


    public bool HasAdditionalInfo => Item?.ClientAdditionalInfos?.Any() ?? false;

    public UserGridDetailViewModel(IMediator mediator, INavigationService navigationService)
    {
        _mediator = mediator;
        _navigationService = navigationService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is Client client)
        {
            Item = client;
            if (client.ClientAdditionalInfos == null)
            {
                var additionalInfos = await _mediator.Send(new GetClientAdditionalInfosQuery(client.ClientId));
                client.ClientAdditionalInfos = additionalInfos.ToList();
                OnPropertyChanged(nameof(Item.ClientAdditionalInfos));
            }
        }
        else if (parameter is string phoneNumber)
        {
            var clients = await _mediator.Send(new GetAllClientsQuery());
            Item = clients.FirstOrDefault(c => c.PhoneNumber == phoneNumber);
            if (Item != null)
            {
                var additionalInfos = await _mediator.Send(new GetClientAdditionalInfosQuery(Item.ClientId));
                Item.ClientAdditionalInfos = additionalInfos.ToList();
                OnPropertyChanged(nameof(Item.ClientAdditionalInfos));
            }
        }
        else if (parameter is int clientId)
        {
            Item = await _mediator.Send(new GetClientByIdQuery(clientId));
            if (Item != null)
            {
                var additionalInfos = await _mediator.Send(new GetClientAdditionalInfosQuery(Item.ClientId));
                Item.ClientAdditionalInfos = additionalInfos.ToList();
                OnPropertyChanged(nameof(Item.ClientAdditionalInfos));
            }
        }
    }

    public void OnNavigatedFrom()
    {
        Item = null;
    }

    [RelayCommand]
    private async Task RefuseServiceAsync()
    {
        if (Item == null) return;

        var dialog = RefuseServiceDialogFactory.Create(
            Item,
            App.MainWindow.Content.XamlRoot
        );

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var refuseService = (RefuseServiceContentDialog)dialog.Content;
            var reason = refuseService.ViewModel.Reason;

            var additionalInfo = new ClientAdditionalInfo
            {
                ClientId = Item.ClientId,
                InfoText = $"Отказ от услуг. \nПричина: {reason}",
                CreatedAt = DateTime.Now
            };

            await _mediator.Send(new CreateAdditionalInfoCommand(additionalInfo));
            await _mediator.Send(new SetClientRejectedCommand(Item.ClientId, reason));

            // Обновляем клиента и его дополнительную информацию
            Item = await _mediator.Send(new GetClientByIdQuery(Item.ClientId));
            var additionalInfos = await _mediator.Send(new GetClientAdditionalInfosQuery(Item.ClientId));
            Item.ClientAdditionalInfos = additionalInfos.ToList();

            OnPropertyChanged(nameof(Item));
            OnPropertyChanged(nameof(HasAdditionalInfo));
            OnPropertyChanged(nameof(Item.ClientAdditionalInfos));
        }
    }

    [RelayCommand]
    private async Task VerifyPhoneAsync()
    {
        if (Item == null) return;

        var isPhoneVerified = await VerifyPhoneNumberAsync(Item.PhoneNumber);
        if (isPhoneVerified)
        {
            await _mediator.Send(new SetClientActiveCommand(Item.ClientId));
            Item = await _mediator.Send(new GetClientByIdQuery(Item.ClientId));
            OnPropertyChanged(nameof(Item));
            OnPropertyChanged(nameof(HasAdditionalInfo));
        }
    }

    [RelayCommand]
    private async Task EditClientAsync()
    {
        if (Item == null) return;

        var dialog = EditClientDialogFactory.Create(
            Item,
            App.MainWindow.Content.XamlRoot,
            "Редактирование клиента"
        );

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var editClient = (EditClientContentDialog)((ContentDialog)dialog).Content;
            var updatedClient = editClient.ViewModel.GetUpdatedClient();
            await _mediator.Send(new UpdateClientCommand(updatedClient));

            if (editClient.ViewModel.IsPhoneNumberChanged())
            {
                var isPhoneVerified = await VerifyPhoneNumberAsync(updatedClient.PhoneNumber);
                if (isPhoneVerified)
                {
                    await _mediator.Send(new SetClientActiveCommand(updatedClient.ClientId));
                }
                else
                {
                    await _mediator.Send(new SetClientDraftCommand(updatedClient.ClientId));
                }
            }


            Item = null;
            OnPropertyChanged(nameof(Item));
            Item = await _mediator.Send(new GetClientByIdQuery(updatedClient.ClientId));
            OnPropertyChanged(nameof(Item));
            OnPropertyChanged(nameof(HasAdditionalInfo));
        }
    }

    private async Task<bool> VerifyPhoneNumberAsync(string phoneNumber)
    {
        var dialog = PhoneVerificationDialogFactory.Create(
            phoneNumber,
            App.MainWindow.Content.XamlRoot
        );

        // TODO: Реализовать отправку SMS
        //await _clientService.SendPhoneConfirmationCodeAsync(phoneNumber);

        var verificationResult = await dialog.ShowAsync();

        if (verificationResult == ContentDialogResult.Secondary)
        {
            return false;
        }

        if (verificationResult == ContentDialogResult.Primary)
        {
            //var phoneVerification = (PhoneVerificationConfirm)dialog.Content;
            //var code = phoneVerification.VerificationCodeInput.Text;
            //return await _clientService.VerifyPhoneConfirmationCodeAsync(phoneNumber, code);
            return true;
        }

        return false;
    }

    [RelayCommand]
    private async Task DeleteClientAsync()
    {
        var dialog = new ContentDialog
        {
            Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            RequestedTheme = App.GetService<IThemeSelectorService>().Theme,
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Title = "Подтверждение",
            Content = "Вы уверены, что хотите удалить этого клиента?",
            PrimaryButtonText = "Удалить",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && Item != null)
        {
            await _mediator.Send(new DeleteClientCommand(Item.ClientId));
            _navigationService.GoBack();
        }
    }

    [RelayCommand]
    private void Finance()
    {
        if (Item != null)
        {
            _navigationService.NavigateTo(typeof(ClientFinanceViewModel).FullName, Item);
        }
    }

    [RelayCommand]
    private async Task AddNoteAsync()
    {
        if (Item == null) return;

        var dialog = RefuseServiceDialogFactory.Create(
            Item,
            App.MainWindow.Content.XamlRoot
        );

        dialog.Title = "Добавить заметку";
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var addNoteDialog = (RefuseServiceContentDialog)dialog.Content;
            var noteText = addNoteDialog.ViewModel.Reason;

            var additionalInfo = new ClientAdditionalInfo
            {
                ClientId = Item.ClientId,
                InfoText = noteText,
                CreatedAt = DateTime.Now
            };

            await _mediator.Send(new CreateAdditionalInfoCommand(additionalInfo));

            var additionalInfos = await _mediator.Send(new GetClientAdditionalInfosQuery(Item.ClientId));
            Item.ClientAdditionalInfos = additionalInfos.ToList();

            OnPropertyChanged(nameof(Item));
            OnPropertyChanged(nameof(HasAdditionalInfo));
            OnPropertyChanged(nameof(Item.ClientAdditionalInfos));
        }
    }
}
