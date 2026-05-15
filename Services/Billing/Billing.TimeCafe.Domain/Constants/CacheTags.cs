namespace Billing.TimeCafe.Domain.Constants;

public static class CacheTags
{
    public const string Balances = "balances";
    public static string Balance(Guid userId) => $"balance:user:{userId}";

    public const string Transactions = "transactions";
    public static string Transaction(Guid transactionId) => $"transaction:{transactionId}";
    public static string TransactionByUser(Guid userId) => $"transaction:user:{userId}";

    public const string Payments = "payments";
    public static string Payment(Guid paymentId) => $"payment:{paymentId}";
    public static string PaymentByUser(Guid userId) => $"payment:user:{userId}";
}
