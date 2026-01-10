namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public record GetBalanceQuery(string UserId) : IRequest<GetBalanceResult>;

public record GetBalanceResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Balance? Balance = null) : ICqrsResultV2
{
    public static GetBalanceResult BalanceNotFound() =>
        new(false, Code: "BalanceNotFound", Message: "Баланс не найден", StatusCode: 404);

    public static GetBalanceResult GetSuccess(Balance balance) =>
        new(true, Message: "Баланс получен", Balance: balance);
}

public class GetBalanceQueryValidator : AbstractValidator<GetBalanceQuery>
{
    public GetBalanceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Баланс не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Баланс не найден");
    }
}

public class GetBalanceQueryHandler(IBalanceRepository repository) : IRequestHandler<GetBalanceQuery, GetBalanceResult>
{
    private readonly IBalanceRepository _repository = repository;

    public async Task<GetBalanceResult> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);
        var balance = await _repository.GetByUserIdAsync(userId, cancellationToken);

        if (balance == null)
        {
            balance = new Balance(userId);
            await _repository.CreateAsync(balance, cancellationToken);
        }

        return GetBalanceResult.GetSuccess(balance);
    }
}
