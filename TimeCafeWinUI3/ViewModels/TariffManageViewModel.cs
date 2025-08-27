﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace TimeCafeWinUI3.ViewModels;

public partial class TariffManageViewModel : ObservableRecipient, INavigationAware
{
    private readonly ITariffCommands _tariffCommands;
    private readonly ITariffQueries _tariffQueries;
    private static int _currentPage = 1;
    private const int PageSize = 16;

    [ObservableProperty] private ObservableCollection<Tariff> source = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string errorMessage;
    [ObservableProperty] private int totalItems;

    public ListView ListView { get; set; }
    public AdaptiveGridView AdaptiveGrid { get; set; }

    public ListViewBase ActiveView => AdaptiveGrid;

    public TariffManageViewModel(ITariffCommands tariffCommands,
        ITariffQueries tariffQueries)
    {
        _tariffCommands = tariffCommands;
        _tariffQueries = tariffQueries;
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

            var items = await _tariffQueries.GetTariffsPageAsync(_currentPage, PageSize);
            var total = await _tariffQueries.GetTotalPageAsync();
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
            await _tariffCommands.DeleteTariffAsync(tariff.TariffId);
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

                var items = await _tariffQueries.GetTariffsPageAsync(_currentPage, PageSize);
                var total = await _tariffQueries.GetTotalPageAsync();
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
