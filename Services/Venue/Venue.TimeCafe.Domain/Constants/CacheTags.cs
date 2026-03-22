namespace Venue.TimeCafe.Domain.Constants;

public static class CacheTags
{
    public const string Tariffs = "tariffs";
    public static string Tariff(Guid id) => $"tariff:{id}";

    public const string Themes = "themes";
    public static string Theme(Guid id) => $"theme:{id}";

    public const string Visits = "visits";
    public static string Visit(Guid id) => $"visit:{id}";
    public static string VisitByUser(Guid userId) => $"visit:user:{userId}";

    public const string Promotions = "promotions";
    public static string Promotion(Guid id) => $"promotion:{id}";
}
