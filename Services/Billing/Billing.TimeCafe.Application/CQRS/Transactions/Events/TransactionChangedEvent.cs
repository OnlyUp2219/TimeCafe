namespace Billing.TimeCafe.Application.CQRS.Transactions.Events;

public sealed record TransactionChangedEvent(Guid TransactionId, Guid UserId) : INotification;
