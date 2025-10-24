namespace UserProfile.TimeCafe.Domain.Constants;

public static class CacheKeys
{

    public const string Profile_All = "user-profile:profile:all";
    private const string ProfilePagesPrefix = "user-profile:profile:page";
    private const string ProfilePagesVersionKey = "user-profile:profile:page:version";
    public static string Profile_ById(string id) => $"user-profile:profile:id:{id}";
    public static string Profile_Page(int page, int version) => $"{ProfilePagesPrefix}:v{version}:p{page}";
    public static string ProfilePagesVersion() => ProfilePagesVersionKey;

}