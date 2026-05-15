namespace Billing.TimeCafe.Domain.Errors;

public sealed class TransactionNotFoundError : Error
{
    public TransactionNotFoundError(Guid transactionId) : base($"Транзакция '{transactionId}' не найдена.")
    {
        Metadata.Add("ErrorCode", "404");
        Metadata.Add("TransactionId", transactionId);
    }
}

public sealed class DuplicateTransactionError : Error
{
    public DuplicateTransactionError(Guid? sourceId = null) : base("Транзакция с таким ключом уже обработана.")
    {
        Metadata.Add("ErrorCode", "409");
        if (sourceId.HasValue)
            Metadata.Add("SourceId", sourceId.Value);
    }
}
