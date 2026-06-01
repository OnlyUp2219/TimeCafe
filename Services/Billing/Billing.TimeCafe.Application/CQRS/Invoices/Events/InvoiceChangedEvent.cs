namespace Billing.TimeCafe.Application.CQRS.Invoices.Events;

public record InvoiceChangedEvent(Guid InvoiceId, Guid? UserId, Guid VisitId) : INotification;
