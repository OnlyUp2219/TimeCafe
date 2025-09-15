using CommunityToolkit.WinUI.UI.Controls;
using System.Diagnostics;

namespace TimeCafe.UI.ViewModels;

public partial class TariffManageViewModel : ObservableRecipient, INavigationAware
{
    private readonly IMediator _mediator;
    private static int _currentPage = 1;
    private const int PageSize = 16;

    [ObservableProperty] private ObservableCollection<Tariff> source = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private int totalItems;

    public ListView ListView { get; set; }
    public AdaptiveGridView AdaptiveGrid { get; set; }

    public ListViewBase ActiveView => AdaptiveGrid;

    public TariffManageViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async void OnNavigatedTo(object parameter)
    {
        await LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
        Source.Clear();
        TotalItems = 0;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            Source.Clear();

            var items = await _mediator.Send(new GetTariffsPageQuery(_currentPage, PageSize));
            var total = await _mediator.Send(new GetTotalPageTariffQuery());
            Debug.WriteLine($"Loaded {items.Count()} items, total: {total}");

            TotalItems = total;

            foreach (var tariff in items)
            {
                Debug.WriteLine($"""
                    Tariff details:
                    - Name: {tariff.TariffName}
                    - DescriptionTitle: {tariff.DescriptionTitle ?? "null"}
                    - Description: {tariff.Description ?? "null"}
                    - Price: {tariff.Price}
                    - BillingType: {tariff.BillingType?.BillingTypeName}
                    - Theme: {tariff.Theme?.ThemeName}
                    - Theme.TechnicalName: {tariff.Theme?.TechnicalName}
                    - ThemeId: {tariff.ThemeId}
                    """);
                Source.Add(tariff);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading data: {ex}");
            ErrorMessage = $"Ошибка при загрузке данных: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteTariffAsync(Tariff tariff)
    {
        if (tariff == null) return;

        try
        {
            IsLoading = true;
            await _mediator.Send(new DeleteTariffСommand(tariff.TariffId));
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при удалении тарифа: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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

                var items = await _mediator.Send(new GetTariffsPageQuery(_currentPage, PageSize));
                var total = await _mediator.Send(new GetTotalPageTariffQuery());
                TotalItems = total;

                foreach (var tariff in items)
                {
                    Source.Add(tariff);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
