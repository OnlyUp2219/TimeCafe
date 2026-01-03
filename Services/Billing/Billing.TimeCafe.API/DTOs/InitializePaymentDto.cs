namespace Billing.TimeCafe.API.DTOs;

public record InitializePaymentDto
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string? ReturnUrl { get; set; }
    public string? Description { get; set; }
}
