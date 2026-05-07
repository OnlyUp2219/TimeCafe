namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record ActivateTariffCommand(Guid TariffId) : ICommand;

public class ActivateTariffCommandValidator : AbstractValidator<ActivateTariffCommand>
{
    public ActivateTariffCommandValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");
    }
}

public class ActivateTariffCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<ActivateTariffCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(ActivateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _uow.Tariffs.GetByIdAsync(request.TariffId, cancellationToken);
            if (existing == null)
                return Result.Fail(new TariffNotFoundError());

            var result = await _uow.Tariffs.ActivateAsync(request.TariffId, cancellationToken);

            if (!result)
                return Result.Fail(new ActivateFailedError());

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

