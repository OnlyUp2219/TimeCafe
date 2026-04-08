using System.ComponentModel.DataAnnotations;

namespace Auth.TimeCafe.Domain.Models;

public sealed class PostmarkOptions
{
    [Required]
    [MinLength(1)]
    public string? ServerToken { get; set; }

    [Required]
    [EmailAddress]
    public string? FromEmail { get; set; }

    [Required]
    [MinLength(1)]
    public string? MessageStream { get; set; }

    [Required]
    [Url]
    public string? FrontendBaseUrl { get; set; }
}
