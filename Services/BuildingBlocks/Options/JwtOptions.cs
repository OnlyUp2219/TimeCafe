using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Options;

public sealed class JwtOptions
{
    [Required]
    [MinLength(1)]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    [Range(1, 3650)]
    public int RefreshTokenExpirationDays { get; set; } = 30;
}