namespace Venue.TimeCafe.Application.CQRS.Visits.Commands;

public record UpdateVisitCommand(string VisitId, string UserId, string TariffId, DateTimeOffset EntryTime, DateTimeOffset? ExitTime, decimal? CalculatedCost, VisitStatus Status) : IRequest<UpdateVisitResult>;

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
        RuleFor(x => x.VisitId)
            .NotEmpty().WithMessage("Посещение не найдено")
           .NotNull().WithMessage("Посещение не найдено")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Посещение не найдено");


        RuleFor(x => x.TariffId)
            .NotEmpty().WithMessage("Тариф не найден")
           .NotNull().WithMessage("Тариф не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тариф не найден");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Пользователь не найден")
           .NotNull().WithMessage("Пользователь не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Пользователь не найден");

        RuleFor(x => x.EntryTime)
            .NotEmpty().WithMessage("Время входа обязательно")
            .Must(t => t != default).WithMessage("Время входа некорректно");

        RuleFor(x => x.ExitTime)
            .Must((cmd, exit) => exit == null || exit.Value != default).WithMessage("Время выхода некорректно")
            .Must((cmd, exit) => exit == null || exit.Value >= cmd.EntryTime).WithMessage("Время выхода не может быть раньше времени входа");

        RuleFor(x => x.CalculatedCost)
            .Must(cost => cost == null || cost >= 0).WithMessage("Стоимость не может быть отрицательной");

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
            var visitId = Guid.Parse(request.VisitId);

            var existing = await _repository.GetByIdAsync(visitId);
            if (existing == null)
                return UpdateVisitResult.VisitNotFound();

            var visit = _mapper.Map<Visit>(existing);
            _mapper.Map(request, visit);

            var tariffId = Guid.Parse(request.TariffId);
            var tariff = await _tariffRepository.GetByIdAsync(tariffId);
            if (tariff == null)
                return UpdateVisitResult.TariffNotFound();

            var updated = await _repository.UpdateAsync(visit);

            if (updated == null)
                return UpdateVisitResult.UpdateFailed();

            return UpdateVisitResult.UpdateSuccess(updated);
        }
        catch (Exception)
        {
            return UpdateVisitResult.UpdateFailed();
        }
    }
}
