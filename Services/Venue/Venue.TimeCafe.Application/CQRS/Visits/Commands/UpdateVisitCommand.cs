namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record UpdateVisitCommand(Guid VisitId, Guid UserId, Guid TariffId, DateTimeOffset EntryTime, DateTimeOffset? ExitTime, decimal? CalculatedCost, VisitStatus Status) : ICommand;

public class UpdateVisitCommandValidator : AbstractValidator<UpdateVisitCommand>
{
    public UpdateVisitCommandValidator()
    {
        RuleFor(x => x.VisitId).ValidGuidEntityId("Посещение не найдено");

        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");

        RuleFor(x => x.UserId).ValidGuidEntityId("Пользователь не найден");

        RuleFor(x => x.EntryTime).ValidEntryTime();

        RuleFor(x => x.ExitTime).ValidExitTime(x => x.EntryTime);

        RuleFor(x => x.CalculatedCost).ValidCalculatedCost();

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Статус посещения некорректен");
    }
}

public class UpdateVisitCommandHandler(IUnitOfWork uow, IMapper mapper, IPublisher publisher) : ICommandHandler<UpdateVisitCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IMapper _mapper = mapper;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(UpdateVisitCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Visits.GetByIdAsync(request.VisitId, cancellationToken);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            var visit = _mapper.Map<Visit>(existing);
            _mapper.Map(request, visit);

            var tariff = await _uow.Tariffs.GetWithThemeByIdAsync(request.TariffId, cancellationToken);
            if (tariff == null)
                return Result.Fail(new TariffNotFoundError());

            var updated = await _uow.Visits.UpdateAsync(visit, cancellationToken);

            if (updated == null)
                return Result.Fail(new VisitUpdateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new VisitChangedEvent(updated.VisitId, updated.UserId), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

