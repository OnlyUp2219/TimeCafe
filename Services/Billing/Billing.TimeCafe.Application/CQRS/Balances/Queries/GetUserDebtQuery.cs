namespace Billing.TimeCafe.Application.CQRS.Balances.Queries;

public record GetUserDebtQuery(Guid UserId) : IRequest<GetUserDebtResult>;

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
            .Must(x => x != Guid.Empty).WithMessage("Пользователь не найден");
    }
}

public class GetUserDebtQueryHandler(IBalanceRepository repository) : IRequestHandler<GetUserDebtQuery, GetUserDebtResult>
{
    private readonly IBalanceRepository _repository = repository;

    public async Task<GetUserDebtResult> Handle(GetUserDebtQuery request, CancellationToken cancellationToken)
    {
        var balance = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (balance == null)
            return GetUserDebtResult.UserNotFound();

        return GetUserDebtResult.GetSuccess(balance.Debt);
    }
}
