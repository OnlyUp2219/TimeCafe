namespace BuildingBlocks.Contracts;

public interface SaveAuditMessage
{
    Guid EventId { get; }
    DateTime CreatedAt { get; }

    string AuditEventJson { get; }
}
