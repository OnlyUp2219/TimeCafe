namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record DeactivateTariffCommand(Guid TariffId) : ICommand;

public class DeactivateTariffCommandValidator : AbstractValidator<DeactivateTariffCommand>
{
    public DeactivateTariffCommandValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");
    }
}

public class DeactivateTariffCommandHandler(ITariffRepository repository) : ICommandHandler<DeactivateTariffCommand>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<Result> Handle(DeactivateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.TariffId);
            if (existing == null)
                return Result.Fail(new TariffNotFoundError());

            var result = await _repository.DeactivateAsync(request.TariffId);

            if (!result)
                return Result.Fail(new DeactivateFailedError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

