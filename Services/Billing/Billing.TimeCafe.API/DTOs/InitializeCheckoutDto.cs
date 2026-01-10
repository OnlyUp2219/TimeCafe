namespace Billing.TimeCafe.API.DTOs;

public record InitializeCheckoutDto
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string? Description { get; set; }
}
