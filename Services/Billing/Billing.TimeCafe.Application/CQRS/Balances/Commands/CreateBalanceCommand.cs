namespace Billing.TimeCafe.Application.CQRS.Balances.Commands;

public record CreateBalanceCommand(string UserId) : IRequest<CreateBalanceResult>;

public record CreateBalanceResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Balance? Balance = null) : ICqrsResultV2
{
    public static CreateBalanceResult AlreadyExists() =>
        new(false, Code: "BalanceAlreadyExists", Message: "Баланс уже существует", StatusCode: 409);

    public static CreateBalanceResult CreateSuccess(Balance balance) =>
        new(true, Message: "Баланс создан", Balance: balance);
}

public class CreateBalanceCommandValidator : AbstractValidator<CreateBalanceCommand>
{
    public CreateBalanceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");
    }
}

public class CreateBalanceCommandHandler(IBalanceRepository repository) : IRequestHandler<CreateBalanceCommand, CreateBalanceResult>
{
    private readonly IBalanceRepository _repository = repository;

    public async Task<CreateBalanceResult> Handle(CreateBalanceCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);

        var exists = await _repository.ExistsAsync(userId, cancellationToken);
        if (exists)
            return CreateBalanceResult.AlreadyExists();

        var balance = new Balance
        {
            UserId = userId,
            CurrentBalance = 0,
            TotalDeposited = 0,
            TotalSpent = 0,
            Debt = 0,
            LastUpdated = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await _repository.CreateAsync(balance, cancellationToken);
        return CreateBalanceResult.CreateSuccess(created);
    }
}
