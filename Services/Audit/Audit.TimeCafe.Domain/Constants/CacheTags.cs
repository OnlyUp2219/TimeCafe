namespace Audit.TimeCafe.Domain.Constants;

public static class CacheTags
{
    public const string AuditLogs = "audit:logs";
    public static string AuditLog(Guid id) => $"audit:log:{id}";
}
