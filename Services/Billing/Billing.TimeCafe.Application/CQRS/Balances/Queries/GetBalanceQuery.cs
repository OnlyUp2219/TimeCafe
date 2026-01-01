namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public record GetBalanceQuery(Guid UserId) : IRequest<GetBalanceResult>;

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
            .Must(x => x != Guid.Empty).WithMessage("Баланс не найден");
    }
}

public class GetBalanceQueryHandler(IBalanceRepository repository) : IRequestHandler<GetBalanceQuery, GetBalanceResult>
{
    private readonly IBalanceRepository _repository = repository;

    public async Task<GetBalanceResult> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (balance == null)
            return GetBalanceResult.BalanceNotFound();

        return GetBalanceResult.GetSuccess(balance);
    }
}
