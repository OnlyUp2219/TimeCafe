namespace UserProfile.TimeCafe.Application.Helpers;

public static class ProfilePhotoUrlMapper
{
    public static string BuildApiUrl(Guid userId) => $"/userprofile/S3/image/{userId}";

    public static Profile WithApiUrl(Profile profile)
    {
        var url = string.IsNullOrWhiteSpace(profile.PhotoUrl)
            ? null
            : BuildApiUrl(profile.UserId);

        return new Profile
        {
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            MiddleName = profile.MiddleName,
            PhotoUrl = url,
            BirthDate = profile.BirthDate,
            CreatedAt = profile.CreatedAt,
            Gender = profile.Gender,
            ProfileStatus = profile.ProfileStatus,
            BanReason = profile.BanReason
        };
    }

    public static IReadOnlyList<Profile> WithApiUrl(IEnumerable<Profile> profiles)
    {
        return profiles.Select(WithApiUrl).ToArray();
    }
}
