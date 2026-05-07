namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record DeleteTariffCommand(Guid TariffId) : ICommand;

public class DeleteTariffCommandValidator : AbstractValidator<DeleteTariffCommand>
{
    public DeleteTariffCommandValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");
    }
}

public class DeleteTariffCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeleteTariffCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(DeleteTariffCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Tariffs.GetByIdAsync(request.TariffId, cancellationToken);
            if (existing == null)
                return Result.Fail(new TariffNotFoundError());

            var result = await _uow.Tariffs.DeleteAsync(request.TariffId, cancellationToken);

            if (!result)
                return Result.Fail(new DeleteFailedError());

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

