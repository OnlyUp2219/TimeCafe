namespace Billing.TimeCafe.Application.DTOs.Payment;

public record AdminPaymentDto(
    Guid PaymentId,
    Guid UserId,
    decimal Amount,
    int PaymentMethod,
    string? ExternalPaymentId,
    int Status,
    Guid? TransactionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorMessage);
