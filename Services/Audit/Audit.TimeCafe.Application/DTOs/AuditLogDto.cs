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
    string? CorrelationId,
    Guid? UserId = null,
    string? OldData = null,
    string? NewData = null,
    string? EnvironmentJson = null,
    string? CustomFieldsJson = null,
    string? Comments = null,
    string? Exception = null);
