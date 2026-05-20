namespace Audit.TimeCafe.Application.Contracts;

public interface IUnitOfWork : BuildingBlocks.Contracts.IUnitOfWork
{
    IAuditLogRepository AuditLogs { get; }
}
