namespace Billing.TimeCafe.Application.CQRS.Payments.Queries;

public record GetPaymentHistoryQuery(
    Guid UserId,
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

    public static GetPaymentHistoryResult WithPayments(List<PaymentDto> payments, int totalCount, int pageSize) =>
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
        RuleFor(x => x.UserId).ValidGuidEntityId("Некорректный ID пользователя");

        RuleFor(x => x.Page).ValidPageNumber();

        RuleFor(x => x.PageSize).ValidPageSize();
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
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var totalCount = await _paymentRepository.GetTotalCountByUserIdAsync(request.UserId, cancellationToken);
        var payments = await _paymentRepository.GetByUserIdAsync(request.UserId, page, pageSize, cancellationToken);

        var paymentDtos = payments.ConvertAll(p => new PaymentDto(
            p.PaymentId,
            p.ExternalPaymentId,
            p.Amount,
            p.Status.ToString(),
            p.CreatedAt,
            p.CompletedAt,
            p.ErrorMessage));

        return GetPaymentHistoryResult.WithPayments(paymentDtos, totalCount, pageSize);
    }
}
