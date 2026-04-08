using System.ComponentModel.DataAnnotations;

namespace UserProfile.TimeCafe.Domain.DTOs;

public class S3Options
{
    [Required]
    [MinLength(1)]
    public string AccessKey { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public string SecretKey { get; set; } = null!;

    [Required]
    [Url]
    public string ServiceUrl { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public string BucketName { get; set; } = null!;
}