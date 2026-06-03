namespace Audit.TimeCafe.Application.CQRS.AuditLogs.Queries;

public sealed record GetAuditLogByIdQuery(Guid Id) : IQuery<AuditLogDto>;

public sealed class GetAuditLogByIdQueryHandler(IUnitOfWork uow)
    : IQueryHandler<GetAuditLogByIdQuery, AuditLogDto>
{
    public async Task<Result<AuditLogDto>> Handle(
        GetAuditLogByIdQuery request, CancellationToken cancellationToken = default)
    {
        var auditLog = await uow.AuditLogs.GetByIdAsync(request.Id, cancellationToken);
        if (auditLog == null)
            return Result.Fail(new Error("Audit log not found"));

        var dto = new AuditLogDto(
            auditLog.Id,
            auditLog.EventType,
            auditLog.Action,
            auditLog.UserName,
            auditLog.MachineName,
            auditLog.DomainName,
            auditLog.Duration,
            auditLog.CreatedAt,
            auditLog.StartDate,
            auditLog.EndDate,
            auditLog.CorrelationId,
            auditLog.OldData,
            auditLog.NewData,
            auditLog.EnvironmentJson,
            auditLog.CustomFieldsJson,
            auditLog.Comments,
            auditLog.Exception);

        return Result.Ok(dto);
    }
}
