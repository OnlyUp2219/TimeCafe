namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record UpdateVisitCommand(Guid VisitId, Guid UserId, Guid TariffId, DateTimeOffset EntryTime, DateTimeOffset? ExitTime, decimal? CalculatedCost, VisitStatus Status) : IRequest<UpdateVisitResult>;

public record UpdateVisitResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Visit? Visit = null) : ICqrsResultV2
{
    public static UpdateVisitResult VisitNotFound() =>
        new(false, Code: "VisitNotFound", Message: "Посещение не найдено", StatusCode: 404);

    public static UpdateVisitResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);

    public static UpdateVisitResult UpdateFailed() =>
        new(false, Code: "UpdateVisitFailed", Message: "Не удалось обновить посещение", StatusCode: 500);

    public static UpdateVisitResult UpdateSuccess(Visit visit) =>
        new(true, Message: "Посещение успешно обновлено", Visit: visit);
}

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

public class UpdateVisitCommandHandler(IVisitRepository repository, ITariffRepository tariffRepository, IMapper mapper) : IRequestHandler<UpdateVisitCommand, UpdateVisitResult>
{
    private readonly IVisitRepository _repository = repository;
    private readonly ITariffRepository _tariffRepository = tariffRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<UpdateVisitResult> Handle(UpdateVisitCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.VisitId);
            if (existing == null)
                return UpdateVisitResult.VisitNotFound();

            var visit = _mapper.Map<Visit>(existing);
            _mapper.Map(request, visit);

            var tariff = await _tariffRepository.GetByIdAsync(request.TariffId);
            if (tariff == null)
                return UpdateVisitResult.TariffNotFound();

            var updated = await _repository.UpdateAsync(visit);

            if (updated == null)
                return UpdateVisitResult.UpdateFailed();

            return UpdateVisitResult.UpdateSuccess(updated);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(UpdateVisitResult.UpdateFailed(), ex);
        }
    }
}
