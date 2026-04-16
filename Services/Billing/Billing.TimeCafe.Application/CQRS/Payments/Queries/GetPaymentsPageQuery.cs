namespace Billing.TimeCafe.Application.CQRS.Payments.Queries;

public record GetPaymentsPageQuery(int Page, int PageSize, Guid? UserId) : IRequest<GetPaymentsPageResult>;

public record GetPaymentsPageResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    List<AdminPaymentDto>? Payments = null,
    int? TotalCount = null,
    int? TotalPages = null) : ICqrsResult
{
    public static GetPaymentsPageResult GetSuccess(List<AdminPaymentDto> payments, int totalCount, int pageSize) =>
        new(true, Message: "Платежи получены",
            Payments: payments,
            TotalCount: totalCount,
            TotalPages: (totalCount + pageSize - 1) / pageSize);
}

public class GetPaymentsPageQueryValidator : AbstractValidator<GetPaymentsPageQuery>
{
    public GetPaymentsPageQueryValidator()
    {
        RuleFor(x => x.Page).ValidPageNumber();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}

public class GetPaymentsPageQueryHandler(IPaymentRepository repository)
    : IRequestHandler<GetPaymentsPageQuery, GetPaymentsPageResult>
{
    public async Task<GetPaymentsPageResult> Handle(GetPaymentsPageQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPageAsync(request.Page, request.PageSize, request.UserId, cancellationToken);

        var dtos = items.ConvertAll(p => new AdminPaymentDto(
            p.PaymentId,
            p.UserId,
            p.Amount,
            (int)p.PaymentMethod,
            p.ExternalPaymentId,
            (int)p.Status,
            p.TransactionId,
            p.CreatedAt,
            p.CompletedAt,
            p.ErrorMessage));

        return GetPaymentsPageResult.GetSuccess(dtos, totalCount, request.PageSize);
    }
}
