namespace Billing.TimeCafe.Domain.Constants;

public static class CacheKeys
{

    public const string Balance_All = "billing:balance:all";
    public static string Balance_ByUserId(Guid userId) => $"billing:balance:user:{userId}";
    public static string Balance_ByUserId(string userId) => $"billing:balance:user:{userId}";


    public const string Transaction_All = "billing:transaction:all";
    public static string Transaction_ById(Guid id) => $"billing:transaction:id:{id}";
    public static string Transaction_ByUserId(Guid userId) => $"billing:transaction:user:{userId}";
    public static string Transaction_ByUserId(string userId) => $"billing:transaction:user:{userId}";
    public static string Transaction_History(Guid userId, int page) => $"billing:transaction:user:{userId}:history:p{page}";


    public const string Payment_All = "billing:payment:all";
    public static string Payment_ById(Guid id) => $"billing:payment:id:{id}";
    public static string Payment_ByUserId(Guid userId) => $"billing:payment:user:{userId}";
    public static string Payment_Pending = "billing:payment:pending";


    public const string Debtors_All = "billing:debtors:all";
    public static string Debt_ByUserId(Guid userId) => $"billing:debt:user:{userId}";
}
