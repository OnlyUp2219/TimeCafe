namespace Billing.TimeCafe.Application.CQRS.Transactions.Queries;

public record GetTransactionHistoryQuery(string UserId, int Page = 1, int PageSize = 10) : IRequest<GetTransactionHistoryResult>;

public record GetTransactionHistoryResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    List<TransactionDto>? Transactions = null,
    int? TotalCount = null,
    int? TotalPages = null) : ICqrsResultV2
{
    public static GetTransactionHistoryResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 404);

    public static GetTransactionHistoryResult GetSuccess(List<TransactionDto> transactions, int totalCount, int pageSize) =>
        new(true, Message: "История транзакций получена",
            Transactions: transactions,
            TotalCount: totalCount,
            TotalPages: (totalCount + pageSize - 1) / pageSize);
}

public class GetTransactionHistoryQueryValidator : AbstractValidator<GetTransactionHistoryQuery>
{
    public GetTransactionHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Страница должна быть больше 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Размер страницы должен быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Размер страницы не может быть больше 100");
    }
}

public class GetTransactionHistoryQueryHandler(ITransactionRepository repository) : IRequestHandler<GetTransactionHistoryQuery, GetTransactionHistoryResult>
{
    private readonly ITransactionRepository _repository = repository;

    public async Task<GetTransactionHistoryResult> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);

        var transactions = await _repository.GetByUserIdAsync(
            userId,
            request.Page,
            request.PageSize,
            cancellationToken);

        if (transactions == null || transactions.Count == 0)
        {
            return GetTransactionHistoryResult.GetSuccess(new List<TransactionDto>(), 0, request.PageSize);
        }

        var totalCount = await _repository.GetTotalCountByUserIdAsync(userId, cancellationToken);

        var dtos = transactions.Select(t => new TransactionDto(
            t.TransactionId,
            t.UserId,
            t.Amount,
            (int)t.Type,
            (int)t.Source,
            (int)t.Status,
            t.Comment,
            t.CreatedAt,
            t.BalanceAfter)).ToList();

        return GetTransactionHistoryResult.GetSuccess(dtos, totalCount, request.PageSize);
    }
}
