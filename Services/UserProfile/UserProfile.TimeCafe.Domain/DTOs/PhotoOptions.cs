using System.ComponentModel.DataAnnotations;

namespace UserProfile.TimeCafe.Domain.DTOs;

public class PhotoOptions
{
    [Required]
    [MinLength(1)]
    public string[] AllowedContentTypes { get; set; } = null!;

    [Range(1, long.MaxValue)]
    public long MaxSizeBytes { get; set; }

    [Range(1, 1440)]
    public int PresignedUrlExpirationMinutes { get; set; } = 15;
}