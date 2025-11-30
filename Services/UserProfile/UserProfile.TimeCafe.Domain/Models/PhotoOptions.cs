namespace UserProfile.TimeCafe.Domain.Models;

public class PhotoOptions
{
    public string[] AllowedContentTypes { get; set; } = null!;
    public long MaxSizeBytes { get; set; }
    public int PresignedUrlExpirationMinutes { get; set; } = 15;
}