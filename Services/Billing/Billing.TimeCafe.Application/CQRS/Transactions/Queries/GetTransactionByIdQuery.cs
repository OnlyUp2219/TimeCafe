namespace Billing.TimeCafe.Application.CQRS.Transactions.Queries;

public sealed record GetTransactionByIdQuery(Guid Id) : IQuery<GetTransactionByIdResponse>;

public sealed record GetTransactionByIdResponse(TransactionDetailDto Transaction);

public sealed class GetTransactionByIdQueryHandler(IUnitOfWork uow)
    : IQueryHandler<GetTransactionByIdQuery, GetTransactionByIdResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<GetTransactionByIdResponse>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken = default)
    {
        var transaction = await _uow.Transactions.GetByIdAsync(request.Id, cancellationToken);
        
        if (transaction == null)
            return Result.Fail(new TransactionNotFoundError(request.Id));

        var dto = new TransactionDetailDto(
            transaction.TransactionId,
            transaction.UserId,
            transaction.Amount,
            (int)transaction.Type,
            (int)transaction.Source,
            transaction.SourceId,
            (int)transaction.Status,
            transaction.Comment,
            transaction.CreatedAt,
            transaction.BalanceAfter);

        return Result.Ok(new GetTransactionByIdResponse(dto));
    }
}
