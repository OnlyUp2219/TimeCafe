namespace TimeCafe.UI.Services;

public class WorkingHoursService : IWorkingHoursService
{
    private readonly ILocalSettingsService _localSettingsService;
    private const string OpenTimeKey = "WorkingHours_OpenTime";
    private const string CloseTimeKey = "WorkingHours_CloseTime";
    private static readonly TimeSpan DefaultOpenTime = new(12, 0, 0); // 12:00
    private static readonly TimeSpan DefaultCloseTime = new(2, 0, 0);  // 02:00

    public WorkingHoursService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task<TimeSpan> GetOpenTimeAsync()
    {
        var openTimeString = await _localSettingsService.ReadSettingAsync<string>(OpenTimeKey);
        if (TimeSpan.TryParse(openTimeString, out var openTime))
        {
            return openTime;
        }
        return DefaultOpenTime;
    }

    public async Task<TimeSpan> GetCloseTimeAsync()
    {
        var closeTimeString = await _localSettingsService.ReadSettingAsync<string>(CloseTimeKey);
        if (TimeSpan.TryParse(closeTimeString, out var closeTime))
        {
            return closeTime;
        }
        return DefaultCloseTime;
    }

    public async Task SetOpenTimeAsync(TimeSpan openTime)
    {
        await _localSettingsService.SaveSettingAsync(OpenTimeKey, openTime.ToString());
    }

    public async Task SetCloseTimeAsync(TimeSpan closeTime)
    {
        await _localSettingsService.SaveSettingAsync(CloseTimeKey, closeTime.ToString());
    }

    public async Task<bool> IsWorkingHoursAsync()
    {
        return await IsWorkingHoursAsync(DateTime.Now);
    }

    public async Task<bool> IsWorkingHoursAsync(DateTime time)
    {
        var openTime = await GetOpenTimeAsync();
        var closeTime = await GetCloseTimeAsync();
        var currentTime = time.TimeOfDay;

        if (closeTime < openTime) // Переход через полночь
        {
            return currentTime >= openTime || currentTime <= closeTime;
        }
        else
        {
            return currentTime >= openTime && currentTime <= closeTime;
        }
    }
}