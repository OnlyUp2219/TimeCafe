namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record DeactivateTariffCommand(Guid TariffId) : ICommand;

public class DeactivateTariffCommandValidator : AbstractValidator<DeactivateTariffCommand>
{
    public DeactivateTariffCommandValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");
    }
}

public class DeactivateTariffCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeactivateTariffCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(DeactivateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _uow.Tariffs.GetByIdAsync(request.TariffId, cancellationToken);
            if (existing == null)
                return Result.Fail(new TariffNotFoundError());

            var result = await _uow.Tariffs.DeactivateAsync(request.TariffId, cancellationToken);

            if (!result)
                return Result.Fail(new DeactivateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new TariffChangedEvent(request.TariffId), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

