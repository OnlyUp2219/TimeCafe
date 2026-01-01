namespace Billing.TimeCafe.Application.CQRS.Balance.Commands;

public record CreateBalanceCommand(Guid UserId) : IRequest<CreateBalanceResult>;

public record CreateBalanceResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Domain.Models.Balance? Balance = null) : ICqrsResultV2
{
    public static CreateBalanceResult AlreadyExists() =>
        new(false, Code: "BalanceAlreadyExists", Message: "Баланс уже существует", StatusCode: 409);

    public static CreateBalanceResult CreateSuccess(Domain.Models.Balance balance) =>
        new(true, Message: "Баланс создан", Balance: balance);
}

public class CreateBalanceCommandValidator : AbstractValidator<CreateBalanceCommand>
{
    public CreateBalanceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
            .Must(x => x != Guid.Empty).WithMessage("Пользователь не найден");
    }
}

public class CreateBalanceCommandHandler(IBalanceRepository repository) : IRequestHandler<CreateBalanceCommand, CreateBalanceResult>
{
    private readonly IBalanceRepository _repository = repository;

    public async Task<CreateBalanceResult> Handle(CreateBalanceCommand request, CancellationToken cancellationToken)
    {
        var exists = await _repository.ExistsAsync(request.UserId, cancellationToken);
        if (exists)
            return CreateBalanceResult.AlreadyExists();

        var balance = new Domain.Models.Balance
        {
            UserId = request.UserId,
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
