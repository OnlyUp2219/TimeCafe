namespace Billing.TimeCafe.Domain.Errors;

public sealed class PaymentNotFoundError : Error
{
    public PaymentNotFoundError(Guid? paymentId = null) : base(paymentId.HasValue ? $"Платеж '{paymentId}' не найден." : "Платеж не найден.")
    {
        Metadata.Add("ErrorCode", "404");
        if (paymentId.HasValue)
            Metadata.Add("PaymentId", paymentId.Value);
    }
}

public sealed class StripeWebhookError : Error
{
    public StripeWebhookError(string message) : base($"Ошибка вебхука Stripe: {message}")
    {
        Metadata.Add("ErrorCode", "400");
    }
}

public sealed class StripeCheckoutError : Error
{
    public StripeCheckoutError(string message) : base($"Ошибка создания сессии оплаты Stripe: {message}")
    {
        Metadata.Add("ErrorCode", "500");
    }
}
