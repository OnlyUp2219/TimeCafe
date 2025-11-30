namespace UserProfile.TimeCafe.Domain.Models;

public class S3Options
{
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string ServiceUrl { get; set; } = null!;
    public string BucketName { get; set; } = null!;
}