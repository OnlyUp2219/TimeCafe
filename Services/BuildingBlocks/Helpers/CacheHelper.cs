namespace BuildingBlocks.Helpers;

public static class CacheHelper
{

    private static readonly DistributedCacheEntryOptions DefaultOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    private static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new DateOnlyJsonConverter(),
            new NullableDateOnlyJsonConverter()
        }
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

            logger.LogDebug("Redis: Удалено {Count} ключей: {Keys}", keys.Length, string.Join(", ", keys));
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
            if (EqualityComparer<T>.Default.Equals(Value, default))
            {
                logger.LogWarning("Попытка записать null в кэш: {Key}", key);
                return;
            }

            var json = JsonSerializer.Serialize(Value, SerializeOptions);

            await cache.SetStringAsync(key, json, options ?? DefaultOptions);

            var ttl = (options ?? DefaultOptions).AbsoluteExpirationRelativeToNow?.TotalSeconds ?? 0;
            logger.LogDebug("Redis: Записан ключ {Key} с TTL {TTL}s", key, ttl);
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
            if (string.IsNullOrEmpty(cached))
            {
                return default;
            }

            logger.LogDebug("Redis: Получены данные для ключа {Key}", key);
            return JsonSerializer.Deserialize<T>(cached, DeserializeOptions);
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
