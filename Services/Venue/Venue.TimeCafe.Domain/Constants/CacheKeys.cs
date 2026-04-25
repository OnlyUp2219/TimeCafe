namespace Venue.TimeCafe.Domain.Constants;

public static class CacheKeys
{
    public const string Tariff_All = "venue:tariff:all";
    public const string Tariff_Active = "venue:tariff:active";
    public static string Tariff_ById(Guid id) => $"venue:tariff:id:{id}";
    public static string Tariff_ByBillingType(int billingType) => $"venue:tariff:billing:{billingType}";
    public static string Tariff_Page(int page, int pageSize) => $"venue:tariff:page:p{page}:s{pageSize}";

    public const string Theme_All = "venue:theme:all";
    public static string Theme_ById(Guid id) => $"venue:theme:id:{id}";

    public const string Visit_Active = "venue:visit:active";
    public static string Visit_ById(Guid id) => $"venue:visit:id:{id}";
    public static string Visit_ActiveByUser(Guid userId) => $"venue:visit:user:{userId}:active";
    public static string Visit_HistoryByUser(Guid userId, int page, int pageSize) => $"venue:visit:user:{userId}:history:p{page}:s{pageSize}";
    public static string Visit_Page(int page, int pageSize) => $"venue:visit:page:p{page}:s{pageSize}";

    public const string Promotion_All = "venue:promotion:all";
    public const string Promotion_Active = "venue:promotion:active";
    public static string Promotion_ById(Guid id) => $"venue:promotion:id:{id}";
    public static string Promotion_ActiveByDate(string date) => $"venue:promotion:active:date:{date}";
}

