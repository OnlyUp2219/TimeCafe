namespace Billing.TimeCafe.Domain.Constants;

public static class CacheKeys
{
    public const string Balance_All = "billing:balance:all";
    public static string Balance_ByUserId(Guid userId) => $"billing:balance:user:{userId}";
    public static string Balance_ByUserId(string userId) => $"billing:balance:user:{userId}";
    public static string Balance_Page(int page, int pageSize) => $"billing:balance:page:p{page}:s{pageSize}";

    public const string Transaction_All = "billing:transaction:all";
    public static string Transaction_ById(Guid id) => $"billing:transaction:id:{id}";
    public static string Transaction_ByUserId(Guid userId) => $"billing:transaction:user:{userId}";
    public static string Transaction_History(Guid userId, int page, int pageSize) => $"billing:transaction:user:{userId}:history:p{page}:s{pageSize}";
    public static string Transaction_Page(int page, int pageSize, Guid? userId = null) => 
        userId.HasValue ? $"billing:transaction:page:p{page}:s{pageSize}:u{userId}" : $"billing:transaction:page:p{page}:s{pageSize}";

    public const string Payment_All = "billing:payment:all";
    public const string Payment_Pending = "billing:payment:pending";
    public static string Payment_ById(Guid id) => $"billing:payment:id:{id}";
    public static string Payment_ByUserId(Guid userId) => $"billing:payment:user:{userId}";
    public static string Payment_Page(int page, int pageSize, Guid? userId = null) =>
        userId.HasValue ? $"billing:payment:page:p{page}:s{pageSize}:u{userId}" : $"billing:payment:page:p{page}:s{pageSize}";

    public const string Debtors_All = "billing:debtors:all";
}
