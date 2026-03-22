namespace UserProfile.TimeCafe.Domain.Constants;

public static class CacheTags
{
    public const string Profiles = "profiles";
    public static string Profile(Guid id) => $"profile:{id}";

    public const string AdditionalInfos = "additional-infos";
    public static string AdditionalInfoByUser(Guid userId) => $"additional-info:user:{userId}";
}
