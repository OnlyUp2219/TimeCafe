namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public record GetUserDebtQuery(string UserId) : IRequest<GetUserDebtResult>;

public record GetUserDebtResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    decimal? Debt = null) : ICqrsResultV2
{
    public static GetUserDebtResult UserNotFound() =>
        new(false, Code: "UserNotFound", Message: "Пользователь не найден", StatusCode: 404);

    public static GetUserDebtResult GetSuccess(decimal debt) =>
        new(true, Message: "Долг получен", Debt: debt);
}

public class GetUserDebtQueryValidator : AbstractValidator<GetUserDebtQuery>
{
    public GetUserDebtQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");
    }
}

public class GetUserDebtQueryHandler(IBalanceRepository repository) : IRequestHandler<GetUserDebtQuery, GetUserDebtResult>
{
    private readonly IBalanceRepository _repository = repository;

    public async Task<GetUserDebtResult> Handle(GetUserDebtQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);
        var balance = await _repository.GetByUserIdAsync(userId, cancellationToken);
        if (balance == null)
        {
            balance = new Balance(userId);
            await _repository.CreateAsync(balance, cancellationToken);
        }

        return GetUserDebtResult.GetSuccess(balance.Debt);
    }
}
