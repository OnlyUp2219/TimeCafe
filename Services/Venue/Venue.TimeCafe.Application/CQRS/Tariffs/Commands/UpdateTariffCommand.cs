namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record UpdateTariffCommand(string TariffId, string Name, string Description, decimal PricePerMinute, BillingType BillingType, string? ThemeId, bool IsActive) : IRequest<UpdateTariffResult>;

public record UpdateTariffResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Tariff? Tariff = null) : ICqrsResultV2
{
    public static UpdateTariffResult TariffNotFound() =>
        new(false, Code: "TariffNotFound", Message: "Тариф не найден", StatusCode: 404);

    public static UpdateTariffResult UpdateFailed() =>
        new(false, Code: "UpdateTariffFailed", Message: "Не удалось обновить тариф", StatusCode: 500);

    public static UpdateTariffResult UpdateSuccess(Tariff tariff) =>
        new(true, Message: "Тариф успешно обновлён", Tariff: tariff);
}

public class UpdateTariffCommandValidator : AbstractValidator<UpdateTariffCommand>
{
    public UpdateTariffCommandValidator()
    {

        RuleFor(x => x.TariffId)
            .NotEmpty().WithMessage("Тариф не найден")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Тариф не найден")
            .Must(x => Guid.TryParse(x, out var guid) && guid != Guid.Empty).WithMessage("Тариф не найден");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название тарифа обязательно")
            .MaximumLength(100).WithMessage("Название не может превышать 100 символов");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Описание не может превышать 500 символов")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PricePerMinute)
            .GreaterThan(0).WithMessage("Цена за минуту должна быть больше 0");

        RuleFor(x => x.BillingType)
            .IsInEnum().WithMessage("Некорректный тип биллинга");

        RuleFor(x => x.ThemeId)
            .Must(x => string.IsNullOrWhiteSpace(x) || (Guid.TryParse(x, out var guid) && guid != Guid.Empty))
            .WithMessage("Темы не существует")
            .When(x => !string.IsNullOrWhiteSpace(x.ThemeId));
    }
}


// Todo : Patch commands
public class UpdateTariffCommandHandler(ITariffRepository repository, IMapper mapper) : IRequestHandler<UpdateTariffCommand, UpdateTariffResult>
{
    private readonly ITariffRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<UpdateTariffResult> Handle(UpdateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffId = Guid.Parse(request.TariffId);

            var existing = await _repository.GetByIdAsync(tariffId);
            if (existing == null)
                return UpdateTariffResult.TariffNotFound();

            var tariff = _mapper.Map<Tariff>(existing);
            _mapper.Map(request, tariff);

            var updated = await _repository.UpdateAsync(tariff);

            if (updated == null)
                return UpdateTariffResult.UpdateFailed();

            return UpdateTariffResult.UpdateSuccess(updated);
        }
        catch (Exception)
        {
            return UpdateTariffResult.UpdateFailed();
        }
    }
}
