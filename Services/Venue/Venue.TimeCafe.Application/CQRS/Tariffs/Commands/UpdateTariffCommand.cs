namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record UpdateTariffCommand(Guid TariffId, string Name, string Description, decimal PricePerMinute, BillingType BillingType, Guid? ThemeId, bool IsActive) : IRequest<UpdateTariffResult>;

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

    public static UpdateTariffResult ThemeNotFound() =>
        new(false, Code: "ThemeNotFound", Message: "Тема не найдена", StatusCode: 404);

    public static UpdateTariffResult UpdateFailed() =>
        new(false, Code: "UpdateTariffFailed", Message: "Не удалось обновить тариф", StatusCode: 500);

    public static UpdateTariffResult UpdateSuccess(Tariff tariff) =>
        new(true, Message: "Тариф успешно обновлён", Tariff: tariff);
}

public class UpdateTariffCommandValidator : AbstractValidator<UpdateTariffCommand>
{
    public UpdateTariffCommandValidator()
    {
        RuleFor(x => x.TariffId).ValidGuidEntityId("Тариф не найден");

        RuleFor(x => x.Name).ValidName("Название тарифа");

        RuleFor(x => x.Description).ValidOptionalDescription();

        RuleFor(x => x.PricePerMinute).ValidPrice();

        RuleFor(x => x.BillingType)
            .IsInEnum().WithMessage("Некорректный тип биллинга");

        RuleFor(x => x.ThemeId).ValidOptionalGuidEntityId("Темы не существует")
            .When(x => x.ThemeId.HasValue);
    }
}


// Todo : Patch commands
public class UpdateTariffCommandHandler(ITariffRepository repository, IThemeRepository themeRepository, IMapper mapper) : IRequestHandler<UpdateTariffCommand, UpdateTariffResult>
{
    private readonly ITariffRepository _repository = repository;
    private readonly IThemeRepository _themeRepository = themeRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<UpdateTariffResult> Handle(UpdateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.TariffId);
            if (existing == null)
                return UpdateTariffResult.TariffNotFound();

            var tariff = _mapper.Map<Tariff>(existing);
            _mapper.Map(request, tariff);

            if (request.ThemeId.HasValue)
            {
                var themeExists = await _themeRepository.GetByIdAsync(request.ThemeId.Value);
                if (themeExists == null)
                    return UpdateTariffResult.ThemeNotFound();
            }

            var updated = await _repository.UpdateAsync(tariff);

            if (updated == null)
                return UpdateTariffResult.UpdateFailed();

            return UpdateTariffResult.UpdateSuccess(updated);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(UpdateTariffResult.UpdateFailed(), ex);
        }
    }
}
