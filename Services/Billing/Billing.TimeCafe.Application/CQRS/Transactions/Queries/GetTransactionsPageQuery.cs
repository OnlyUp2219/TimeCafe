namespace Billing.TimeCafe.Application.CQRS.Transactions.Queries;

public sealed record GetTransactionsPageQuery(int Page, int PageSize, Guid? UserId) : IQuery<PagedResponse<TransactionDto>>;

public sealed class GetTransactionsPageQueryHandler(IUnitOfWork uow)
    : IQueryHandler<GetTransactionsPageQuery, PagedResponse<TransactionDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<PagedResponse<TransactionDto>>> Handle(GetTransactionsPageQuery request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (items, totalCount) = await _uow.Transactions.GetPageAsync(request.Page, request.PageSize, request.UserId, cancellationToken);

            var dtos = items.ConvertAll(t => new TransactionDto(
                t.TransactionId,
                t.UserId,
                t.Amount,
                (int)t.Type,
                (int)t.Source,
                t.SourceId,
                (int)t.Status,
                t.Comment,
                t.CreatedAt,
                t.BalanceAfter));

            var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;

            return Result.Ok(new PagedResponse<TransactionDto>(
                dtos, 
                new PageMetadata(request.Page, request.PageSize, totalCount, totalPages)));
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}
