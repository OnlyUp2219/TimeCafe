namespace Audit.TimeCafe.Application.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    string EventType,
    string Action,
    string UserName,
    string MachineName,
    string DomainName,
    long Duration,
    DateTime CreatedAt,
    DateTime? StartDate,
    DateTime? EndDate,
    string? CorrelationId);
