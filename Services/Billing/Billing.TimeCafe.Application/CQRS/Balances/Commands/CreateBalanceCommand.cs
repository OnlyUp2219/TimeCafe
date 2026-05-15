namespace Billing.TimeCafe.Application.CQRS.Balances.Commands;

public sealed record CreateBalanceCommand(Guid UserId) : ICommand<CreateBalanceResponse>;

public sealed record CreateBalanceResponse(Guid UserId, decimal CurrentBalance);

public sealed class CreateBalanceCommandValidator : AbstractValidator<CreateBalanceCommand>
{
    public CreateBalanceCommandValidator()
    {
        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");
    }
}

public sealed class CreateBalanceCommandHandler(IUnitOfWork uow) : ICommandHandler<CreateBalanceCommand, CreateBalanceResponse>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<Result<CreateBalanceResponse>> Handle(CreateBalanceCommand request, CancellationToken cancellationToken = default)
    {
        var exists = await _uow.Balances.ExistsAsync(request.UserId, cancellationToken);
        if (exists)
        {
            var existing = await _uow.Balances.GetByIdAsync(request.UserId, cancellationToken);
            return Result.Ok(new CreateBalanceResponse(existing!.UserId, existing.CurrentBalance));
        }

        var balance = Balance.Create(request.UserId);
        await _uow.Balances.CreateAsync(balance, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new CreateBalanceResponse(balance.UserId, balance.CurrentBalance));
    }
}
