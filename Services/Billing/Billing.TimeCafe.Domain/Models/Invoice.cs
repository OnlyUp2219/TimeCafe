namespace Billing.TimeCafe.Domain.Models;

public class Invoice
{
    public Invoice()
    {
        InvoiceId = Guid.NewGuid();
    }

    public Invoice(Guid invoiceId)
    {
        InvoiceId = invoiceId;
    }

    public Guid InvoiceId { get; set; }
    public Guid? UserId { get; set; }
    public Guid VisitId { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public PaymentMethod? PaymentMethod { get; set; }
    public string? StripeSessionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PaidAt { get; set; }

    public static Result<Invoice> Create(Guid? userId, Guid visitId, decimal totalAmount)
    {
        if (totalAmount < 0)
            return Result.Fail<Invoice>(new InvalidInvoiceAmountError());

        if (visitId == Guid.Empty)
            return Result.Fail<Invoice>(new EmptyVisitIdError());

        return Result.Ok(new Invoice
        {
            UserId = userId,
            VisitId = visitId,
            TotalAmount = totalAmount,
            Status = InvoiceStatus.Pending
        });
    }

    public Result Pay(PaymentMethod method, DateTimeOffset? paidAt = null)
    {
        if (Status == InvoiceStatus.Paid)
            return Result.Fail(new InvoiceAlreadyPaidError());

        if (Status != InvoiceStatus.Pending)
            return Result.Fail(new InvalidInvoiceStatusError($"Невозможно оплатить инвойс в статусе {Status}"));

        Status = InvoiceStatus.Paid;
        PaymentMethod = method;
        PaidAt = paidAt ?? DateTimeOffset.UtcNow;
        return Result.Ok();
    }

    public Result Cancel()
    {
        if (Status == InvoiceStatus.Cancelled)
            return Result.Ok();

        if (Status != InvoiceStatus.Pending)
            return Result.Fail(new InvalidInvoiceStatusError($"Невозможно отменить инвойс в статусе {Status}"));

        Status = InvoiceStatus.Cancelled;
        return Result.Ok();
    }
}
