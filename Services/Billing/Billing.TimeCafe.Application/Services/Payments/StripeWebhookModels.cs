using System.Text.Json.Serialization;

namespace Billing.TimeCafe.Application.Services.Payments;

public class StripeWebhookPayload
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("data")]
    public StripeWebhookData? Data { get; set; }
}

public class StripeWebhookData
{
    [JsonPropertyName("object")]
    public StripePaymentIntentObject? Object { get; set; }
}

public class StripePaymentIntentObject
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }
}
