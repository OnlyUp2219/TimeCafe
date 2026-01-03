namespace Billing.TimeCafe.Application.CQRS.Payments.Queries;

public record GetPaymentHistoryQuery(
    string UserId,
    int Page = 1,
    int PageSize = 20) : IRequest<GetPaymentHistoryResult>;

public record GetPaymentHistoryResult(
    bool Success,
    List<PaymentDto>? Payments = null,
    int TotalCount = 0,
    int TotalPages = 0,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null) : ICqrsResultV2
{
    public static GetPaymentHistoryResult InvalidUserId() =>
        new(false, Code: "InvalidUserId", Message: "Некорректный ID пользователя", StatusCode: 400);

    public static GetPaymentHistoryResult WithPayments(List<PaymentDto> payments, int totalCount, int page, int pageSize) =>
        new(true, payments, totalCount, (totalCount + pageSize - 1) / pageSize);
}

public record PaymentDto(
    Guid PaymentId,
    string? ExternalPaymentId,
    decimal Amount,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorMessage);

public class GetPaymentHistoryQueryValidator : AbstractValidator<GetPaymentHistoryQuery>
{
    public GetPaymentHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Некорректный ID пользователя")
            .Must(x => Guid.TryParse(x, out _)).WithMessage("Некорректный ID пользователя");

        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Некорректный номер страницы");
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100).WithMessage("Размер страницы от 1 до 100");
    }
}

public class GetPaymentHistoryQueryHandler(
    IPaymentRepository paymentRepository,
    ILogger<GetPaymentHistoryQueryHandler> logger) : IRequestHandler<GetPaymentHistoryQuery, GetPaymentHistoryResult>
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly ILogger _logger = logger;

    public async Task<GetPaymentHistoryResult> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            _logger.LogWarning("Invalid user ID format: {UserId}", request.UserId);
            return GetPaymentHistoryResult.InvalidUserId();
        }

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var totalCount = await _paymentRepository.GetTotalCountByUserIdAsync(userId, cancellationToken);
        var payments = await _paymentRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);

        var paymentDtos = payments.Select(p => new PaymentDto(
            p.PaymentId,
            p.ExternalPaymentId,
            p.Amount,
            p.Status.ToString(),
            p.CreatedAt,
            p.CompletedAt,
            p.ErrorMessage)).ToList();

        return GetPaymentHistoryResult.WithPayments(paymentDtos, totalCount, page, pageSize);
    }
}
