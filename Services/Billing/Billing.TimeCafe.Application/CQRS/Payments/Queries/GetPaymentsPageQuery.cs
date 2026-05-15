namespace Billing.TimeCafe.Application.CQRS.Payments.Queries;

public sealed record GetPaymentsPageQuery(int Page, int PageSize, Guid? UserId) : IQuery<PagedResponse<AdminPaymentDto>>;

public sealed class GetPaymentsPageQueryHandler(IUnitOfWork uow)
    : IQueryHandler<GetPaymentsPageQuery, PagedResponse<AdminPaymentDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<AdminPaymentDto>>> Handle(GetPaymentsPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (items, totalCount) = await _uow.Payments.GetPageAsync(request.Page, request.PageSize, request.UserId, cancellationToken);

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

            var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

            return Result.Ok(new PagedResponse<AdminPaymentDto>(
                dtos, 
                new PageMetadata(request.Page, request.PageSize, totalCount, totalPages)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
