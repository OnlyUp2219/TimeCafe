namespace Audit.TimeCafe.Application.Contracts;

public interface IAuditLogRepository : IRepository<AuditLog, Guid>
{
    Task<(List<AuditLog> Items, int TotalCount)> GetPageAsync(
        int page, int pageSize, string? eventType, string? userName, CancellationToken cancellationToken = default);
}
