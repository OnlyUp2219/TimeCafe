using Auth.TimeCafe.Domain.Contracts;

using Microsoft.Extensions.Caching.Memory;

namespace Auth.TimeCafe.Infrastructure.Services.Phone;

// TODO(SECURITY-THROTTLE v2)
// Переработать в универсальный механизм:
// 1. Вынести параметры (MaxAttempts, AttemptWindowMinutes, LockoutMinutes) в конфиг: SecurityThrottle:Attempts:sms_verify
// 2. Добавить поддержку lockout:
//    - Метод IsLockedOut(userId, phoneNumber) / GetRemainingLockoutSeconds
//    - При достижении MaxAttempts ставить lockout ключ: lockout:sms_verify:{userId}:{phoneNumber}
// 3. Унифицировать ключи:
//    attempts:sms_verify:{userId}:{phoneNumber}, lockout:sms_verify:{userId}:{phoneNumber}
// 4. Заменить специфичный класс на ISecurityThrottle с категориями (sms_verify, password_login, email_reset, sms_send, email_send)
// 5. Рассмотреть переход на IDistributedCache (Redis) для масштабирования
// 6. Добавить unit-тесты: достижение лимита, истечение окна, lockout, параллельные запросы
// 7. Сделать возможность сброса (ResetAttempts) по событию успешной верификации
// 8. В будущем объединить с RateLimiter (частота успешных операций) — создать единый сервис SecurityThrottle
// 9. Логировать события превышения лимита и установку lockout (категория/ключ)
// 10. Публичный контракт: CanAttempt, RecordFailedAttempt, ResetAttempts, GetRemainingAttempts, IsLockedOut, GetRemainingLockoutSeconds
// Текущая реализация — только счетчик попыток без lockout.
public class SmsVerificationAttemptTracker(IMemoryCache cache) : ISmsVerificationAttemptTracker
{
    private readonly IMemoryCache _cache = cache;
    private const int MaxAttempts = 5;
    private const int AttemptWindowMinutes = 10;

    public bool CanVerifyCode(string userId, string phoneNumber)
    {
        var attempts = GetCurrentAttempts(userId, phoneNumber);
        return attempts < MaxAttempts;
    }

    public void RecordFailedAttempt(string userId, string phoneNumber)
    {
        var key = GetCacheKey(userId, phoneNumber);
        var attempts = GetCurrentAttempts(userId, phoneNumber);

        _cache.Set(key, attempts + 1, TimeSpan.FromMinutes(AttemptWindowMinutes));
    }

    public int GetRemainingAttempts(string userId, string phoneNumber)
    {
        var attempts = GetCurrentAttempts(userId, phoneNumber);
        return Math.Max(0, MaxAttempts - attempts);
    }

    public void ResetAttempts(string userId, string phoneNumber)
    {
        var key = GetCacheKey(userId, phoneNumber);
        _cache.Remove(key);
    }

    private int GetCurrentAttempts(string userId, string phoneNumber)
    {
        var key = GetCacheKey(userId, phoneNumber);
        return _cache.TryGetValue(key, out int attempts) ? attempts : 0;
    }

    private static string GetCacheKey(string userId, string phoneNumber)
        => $"sms_verify_attempts:{userId}:{phoneNumber}";
}
