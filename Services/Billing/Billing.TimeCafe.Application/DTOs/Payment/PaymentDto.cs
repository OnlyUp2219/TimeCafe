namespace Billing.TimeCafe.Application.DTOs.Payment;

public sealed record PaymentDto(
    Guid PaymentId,
    string? ExternalPaymentId,
    decimal Amount,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorMessage);
