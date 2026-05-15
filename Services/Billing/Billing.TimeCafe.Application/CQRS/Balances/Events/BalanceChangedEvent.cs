namespace Billing.TimeCafe.Application.CQRS.Balances.Events;

public sealed record BalanceChangedEvent(Guid UserId) : INotification;
