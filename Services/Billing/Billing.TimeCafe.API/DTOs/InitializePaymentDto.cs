namespace Billing.TimeCafe.API.DTOs;

public record InitializePaymentDto
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReturnUrl { get; set; }
    public string? Description { get; set; }
}
