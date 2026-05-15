namespace Billing.TimeCafe.Application.CQRS.Payments.Queries;

public sealed record GetPaymentHistoryQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20) : IQuery<GetPaymentHistoryResponse>;

public sealed record GetPaymentHistoryResponse(
    List<PaymentDto> Payments,
    int TotalCount,
    int TotalPages);

public sealed class GetPaymentHistoryQueryHandler(
    IUnitOfWork uow) : IQueryHandler<GetPaymentHistoryQuery, GetPaymentHistoryResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetPaymentHistoryResponse>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken = default)
    {
        var totalCount = await _uow.Payments.GetTotalCountByUserIdAsync(request.UserId, cancellationToken);
        var payments = await _uow.Payments.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, cancellationToken);

        var paymentDtos = payments.ConvertAll(p => new PaymentDto(
            p.PaymentId,
            p.ExternalPaymentId,
            p.Amount,
            p.Status.ToString(),
            p.CreatedAt,
            p.CompletedAt,
            p.ErrorMessage));

        var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

        return Result.Ok(new GetPaymentHistoryResponse(paymentDtos, totalCount, totalPages));
    }
}
