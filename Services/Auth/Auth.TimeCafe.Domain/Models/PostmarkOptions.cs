namespace Auth.TimeCafe.Domain.Models;

public sealed class PostmarkOptions
{
    public string? ServerToken { get; set; }
    public string? FromEmail { get; set; }
    public string? MessageStream { get; set; }
    public string? FrontendBaseUrl { get; set; }
}
