namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record UpdateTariffCommand(Guid TariffId, string Name, string Description, decimal PricePerMinute, BillingType BillingType, Guid? ThemeId, bool IsActive) : ICommand<Tariff>;

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
public class UpdateTariffCommandHandler(ITariffRepository repository, IThemeRepository themeRepository, IMapper mapper) : ICommandHandler<UpdateTariffCommand, Tariff>
{
    private readonly ITariffRepository _repository = repository;
    private readonly IThemeRepository _themeRepository = themeRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<Tariff>> Handle(UpdateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.TariffId);
            if (existing == null)
                return Result.Fail(new TariffNotFoundError());

            var tariff = _mapper.Map<Tariff>(existing);
            _mapper.Map(request, tariff);

            if (request.ThemeId.HasValue)
            {
                var themeExists = await _themeRepository.GetByIdAsync(request.ThemeId.Value);
                if (themeExists == null)
                    return Result.Fail(new ThemeNotFoundError());
            }

            var updated = await _repository.UpdateAsync(tariff);

            if (updated == null)
                return Result.Fail(new UpdateFailedError());

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

