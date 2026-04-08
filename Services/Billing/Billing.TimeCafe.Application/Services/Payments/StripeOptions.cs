using System.ComponentModel.DataAnnotations;

namespace Billing.TimeCafe.Application.Services.Payments;

public class StripeOptions
{
    [Required]
    [MinLength(1)]
    public string PublishableKey { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string SecretKey { get; set; } = string.Empty;

    public string? WebhookSecret { get; set; }

    [Required]
    [MinLength(3)]
    public string DefaultCurrency { get; set; } = "rub";

    [Url]
    public string? DefaultReturnUrl { get; set; }

    [Url]
    public string? CheckoutSuccessUrl { get; set; }

    [Url]
    public string? CheckoutCancelUrl { get; set; }
}
