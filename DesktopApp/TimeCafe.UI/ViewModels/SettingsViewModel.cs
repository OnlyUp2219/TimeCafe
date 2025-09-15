using System.Reflection;
using System.Windows.Input;
using TimeCafe.UI.Utilities.Helpers;
using Windows.ApplicationModel;

namespace TimeCafe.UI.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IWorkingHoursService _workingHoursService;
    private readonly ILocalSettingsService _localSettingsService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private TimeSpan _openTime;

    [ObservableProperty]
    private TimeSpan _closeTime;

    [ObservableProperty]
    private int _minimumEntryMinutes = 20;

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public ICommand SaveWorkingHoursCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IWorkingHoursService workingHoursService, ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _workingHoursService = workingHoursService;
        _localSettingsService = localSettingsService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        if (App.MainWindow?.Content is FrameworkElement rootElement)
        {
            rootElement.ActualThemeChanged += RootElement_ActualThemeChanged;
        }

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        SaveWorkingHoursCommand = new AsyncRelayCommand(SaveWorkingHoursAsync);

        // Загружаем настройки рабочего времени
        _ = LoadWorkingHoursAsync();
        // Загружаем минимальное время входа
        _ = LoadMinimumEntryMinutesAsync();
    }

    private async Task LoadWorkingHoursAsync()
    {
        try
        {
            OpenTime = await _workingHoursService.GetOpenTimeAsync();
            CloseTime = await _workingHoursService.GetCloseTimeAsync();
        }
        catch (Exception ex)
        {
            // TODO: Обработка ошибок
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настроек рабочего времени: {ex.Message}");
        }
    }

    private async Task SaveWorkingHoursAsync()
    {
        try
        {
            await _workingHoursService.SetOpenTimeAsync(OpenTime);
            await _workingHoursService.SetCloseTimeAsync(CloseTime);
        }
        catch (Exception ex)
        {
            // TODO: Обработка ошибок
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек рабочего времени: {ex.Message}");
        }
    }

    private void RootElement_ActualThemeChanged(FrameworkElement sender, object args)
    {
        if (_themeSelectorService.Theme == ElementTheme.Default)
        {
            ElementTheme = sender.ActualTheme;
        }
        else
        {
            ElementTheme = _themeSelectorService.Theme;
        }
    }

    private async Task LoadMinimumEntryMinutesAsync()
    {
        var value = await _localSettingsService.ReadSettingAsync<int?>("MinimumEntryMinutes");
        if (value.HasValue && value.Value > 0)
            MinimumEntryMinutes = value.Value;
    }

    [RelayCommand]
    private async Task SaveMinimumEntryMinutesAsync()
    {
        await _localSettingsService.SaveSettingAsync("MinimumEntryMinutes", MinimumEntryMinutes);
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
