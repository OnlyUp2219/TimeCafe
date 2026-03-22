namespace UserProfile.TimeCafe.Domain.Constants;

public static class CacheKeys
{
    private const string ProfilePagesVersionKey = "user-profile:profile:page:version";

    public const string Profile_All = "user-profile:profile:all";
    public static string Profile_ById(Guid id) => $"user-profile:profile:id:{id}";
    public static string Profile_ById(string id) => $"user-profile:profile:id:{id}";
    public static string Profile_Page(int page) => $"user-profile:profile:page:p{page}";
    public static string Profile_Page(int page, int version) => Profile_Page(page);
    public static string ProfilePagesVersion() => ProfilePagesVersionKey;

    public const string AdditionalInfo_All = "user-profile:additionalinfo:all";
    public static string AdditionalInfo_ById(string id) => $"user-profile:additionalinfo:id:{id}";
    public static string AdditionalInfo_ById(Guid id) => $"user-profile:additionalinfo:id:{id}";
    public static string AdditionalInfo_ByUserId(string userId) => $"user-profile:additionalinfo:user:{userId}";
    public static string AdditionalInfo_ByUserId(Guid userId) => $"user-profile:additionalinfo:user:{userId}";
}
