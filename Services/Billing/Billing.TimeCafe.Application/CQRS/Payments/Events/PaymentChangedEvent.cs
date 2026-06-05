namespace Billing.TimeCafe.Application.CQRS.Payments.Events;

public sealed record PaymentChangedEvent(Guid PaymentId, Guid? UserId, Guid? TransactionId = null) : INotification;
