using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using TimeCafe.Common.Utilities;

namespace TimeCafe.Common.Helpers;

public static class CacheHelper
{

    private static readonly DistributedCacheEntryOptions DefaultOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };


    /// <summary>
    /// Удаляет несколько ключей из кэша параллельно.
    /// Если Redis недоступен, ошибки логируются, но метод не кидает исключения.
    /// </summary>
    public static async Task RemoveKeysAsync(
        IDistributedCache cache,
        ILogger logger,
        params string[] keys)
    {
        if (keys == null || keys.Length == 0)
            return;

        try
        {
            var tasks = keys.Select(k => cache.RemoveAsync(k));
            await Task.WhenAll(tasks);

            logger.LogInformation("Redis: RemoveKeysAsync сработал, удалены ключи: {Keys}", string.Join(", ", keys));
            logger.LogInformation("Redis: Количество удалённых ключей = {Count}", keys.Length);
            logger.LogInformation("Redis: Операция удаления завершена успешно");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении ключей из кэша: {Keys}", string.Join(", ", keys));
        }
    }

    /// <summary>
    /// Записывает объект в кэш, если TTL не указан, по умолчанию 5 минут.
    /// Если Redis недоступен, ошибки логируются, но метод не кидает исключения.
    /// </summary>
    public static async Task SetAsync<T>(
        IDistributedCache cache,
        ILogger logger,
        string key,
        T Value,
        DistributedCacheEntryOptions? options = null)
    {
        try
        {
            if (Value == null)
            {
                logger.LogWarning("Попытка записать null в кэш: {Key}", key);
                return;
            }

            var json = JsonSerializer.Serialize(Value,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                });

            await cache.SetStringAsync(key, json, options ?? DefaultOptions);

            logger.LogInformation("Redis: Метод SetAsync с ключом {Key} успешно выполнен", key);
            logger.LogInformation("Redis: Значение записано в кэш");
            logger.LogInformation("Redis: Время жизни ключа (TTL) = {TTL} секунд", (options ?? DefaultOptions).AbsoluteExpirationRelativeToNow?.TotalSeconds ?? 0);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при записи ключей в кэш: {Key}", key);
        }
    }

    /// <summary>
    /// Получает объект из кэша.
    /// Если Redis недоступен, ошибки логируются, но метод не кидает исключения.
    /// </summary>
    public static async Task<T?> GetAsync<T>(
        IDistributedCache cache,
        ILogger logger,
        string key
        )
    {
        try
        {
            var cached = await cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cached))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                options.Converters.Add(new DateOnlyJsonConverter());
                options.Converters.Add(new NullableDateOnlyJsonConverter());

                logger.LogInformation("Redis: GetAsync успешно получил данные для ключа {Key}", key);
                logger.LogInformation("Redis: Данные извлечены из кэша");

                return JsonSerializer.Deserialize<T>(cached, options);
            }
            else
            {
                return default;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка получения кэша: {Key}", key);
            return default;
        }
    }

    public static async Task InvalidatePagesCacheAsync(IDistributedCache cache, string PageVersion)
    {
        var versionStr = await cache.GetStringAsync(PageVersion);
        var version = int.TryParse(versionStr, out var v) ? v + 1 : 2;
        await cache.SetStringAsync(PageVersion, version.ToString());
    }
}
