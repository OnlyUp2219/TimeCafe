namespace Audit.TimeCafe.Domain.Constants;

public static class CacheKeys
{
    public static string AuditLog_ById(Guid id) => $"audit:log:id:{id}";
}
