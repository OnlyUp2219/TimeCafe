namespace Venue.TimeCafe.Domain.Constants;

public static class CacheKeys
{
    public const string Tariff_All = "venue:tariff:all";
    public const string Tariff_Active = "venue:tariff:active";
    private const string TariffPagesPrefix = "venue:tariff:page";
    private const string TariffPagesVersionKey = "venue:tariff:page:version";
    public static string Tariff_ById(Guid id) => $"venue:tariff:id:{id}";
    public static string Tariff_ByBillingType(int billingType) => $"venue:tariff:billing:{billingType}";
    public static string Tariff_Page(int page, int version) => $"{TariffPagesPrefix}:v{version}:p{page}";
    public static string TariffPagesVersion() => TariffPagesVersionKey;


    public const string Theme_All = "venue:theme:all";
    public static string Theme_ById(Guid id) => $"venue:theme:id:{id}";


    public const string Visit_Active = "venue:visit:active";
    public static string Visit_ById(Guid id) => $"venue:visit:id:{id}";
    public static string Visit_ByUser(Guid userId) => $"venue:visit:user:{userId}";
    public static string Visit_ActiveByUser(Guid userId) => $"venue:visit:user:{userId}:active";
    public static string Visit_HistoryByUser(Guid userId, int page) => $"venue:visit:user:{userId}:history:p{page}";

    public const string Promotion_All = "venue:promotion:all";
    public const string Promotion_Active = "venue:promotion:active";
    public static string Promotion_ById(Guid id) => $"venue:promotion:id:{id}";
    public static string Promotion_ActiveByDate(string date) => $"venue:promotion:active:date:{date}";
}
