namespace Audit.TimeCafe.Domain.Models;

public class AuditLog
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public long Duration { get; set; }

    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public string? EnvironmentJson { get; set; }
    public string? CustomFieldsJson { get; set; }
    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CorrelationId { get; set; }

    public AuditLog(Guid eventId, DateTime createdAt, AuditEvent auditEvent)
    {
        Id = eventId;
        CreatedAt = createdAt;

        EventType = auditEvent.EventType ?? "Unknown";
        UserName = auditEvent.Environment?.UserName ?? "Unknown";
        MachineName = auditEvent.Environment?.MachineName ?? "Unknown";
        DomainName = auditEvent.Environment?.DomainName ?? "Unknown";
        Exception = auditEvent.Environment?.Exception;
        Duration = auditEvent.Duration;

        Action = auditEvent.CustomFields.TryGetValue("CommandName", out var commandName)
            ? commandName?.ToString() ?? "Unknown"
            : auditEvent.Environment?.CallingMethodName ?? "Unknown";

        StartDate = auditEvent.StartDate;
        EndDate = auditEvent.EndDate;

        OldData = auditEvent.Target?.Old != null ? JsonSerializer.Serialize(auditEvent.Target.Old) : null;
        NewData = auditEvent.Target?.New != null ? JsonSerializer.Serialize(auditEvent.Target.New) : null;
        EnvironmentJson = auditEvent.Environment != null ? JsonSerializer.Serialize(auditEvent.Environment) : null;
        CustomFieldsJson = auditEvent.CustomFields.Count > 0 ? JsonSerializer.Serialize(auditEvent.CustomFields) : null;
        Comments = auditEvent.Comments.Count > 0 ? JsonSerializer.Serialize(auditEvent.Comments) : null;
    }

    public AuditLog() { }
}
