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

public class UpdateVisitCommandHandler(IVisitRepository repository, ITariffRepository tariffRepository, IMapper mapper) : ICommandHandler<UpdateVisitCommand>
{
    private readonly IVisitRepository _repository = repository;
    private readonly ITariffRepository _tariffRepository = tariffRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result> Handle(UpdateVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.VisitId);
            if (existing == null)
                return Result.Fail(new VisitNotFoundError());

            var visit = _mapper.Map<Visit>(existing);
            _mapper.Map(request, visit);

            var tariff = await _tariffRepository.GetByIdAsync(request.TariffId);
            if (tariff == null)
                return Result.Fail(new TariffNotFoundError());

            var updated = await _repository.UpdateAsync(visit);

            if (updated == null)
                return Result.Fail(new UpdateFailedError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

