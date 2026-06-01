namespace Billing.TimeCafe.Domain.Errors;

public sealed class InvoiceNotFoundError : Error
{
    public InvoiceNotFoundError()
        : base("Счёт на оплату не найден.")
    {
        Metadata.Add("ErrorCode", "404");
    }
}

public sealed class InvoiceAlreadyPaidError : Error
{
    public InvoiceAlreadyPaidError()
        : base("Этот счёт уже был успешно оплачен.")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class InvalidInvoiceStatusError : Error
{
    public InvalidInvoiceStatusError(string message)
        : base(message)
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class InvalidInvoiceAmountError : Error
{
    public InvalidInvoiceAmountError()
        : base("Сумма счёта не может быть отрицательной.")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class EmptyVisitIdError : Error
{
    public EmptyVisitIdError()
        : base("Идентификатор визита не может быть пустым.")
    {
        Metadata.Add("ErrorCode", "400");
    }
}
