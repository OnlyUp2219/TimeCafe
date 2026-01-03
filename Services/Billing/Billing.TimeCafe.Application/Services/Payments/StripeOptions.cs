namespace Billing.TimeCafe.Application.Services.Payments;

public class StripeOptions
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? WebhookSecret { get; set; }
    public string DefaultCurrency { get; set; } = "rub";
    public string? DefaultReturnUrl { get; set; }
}
