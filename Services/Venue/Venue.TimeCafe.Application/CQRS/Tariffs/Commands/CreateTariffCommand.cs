namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record CreateTariffCommand(
    string Name,
    string? Description,
    decimal PricePerMinute,
    BillingType BillingType,
    Guid? ThemeId,
    bool IsActive = true) : ICommand<Tariff>;

public class CreateTariffCommandValidator : AbstractValidator<CreateTariffCommand>
{
    public CreateTariffCommandValidator()
    {
        RuleFor(x => x.Name).ValidName("Название тарифа");

        RuleFor(x => x.Description).ValidOptionalDescription();

        RuleFor(x => x.PricePerMinute).ValidPrice();

        RuleFor(x => x.BillingType)
            .IsInEnum().WithMessage("Неверный тип биллинга");

        RuleFor(x => x.ThemeId).ValidOptionalGuidEntityId("Неверный идентификатор темы");
    }
}

public class CreateTariffCommandHandler(ITariffRepository repository, IThemeRepository themeRepository) : ICommandHandler<CreateTariffCommand, Tariff>
{
    private readonly ITariffRepository _repository = repository;
    private readonly IThemeRepository _themeRepository = themeRepository;

    public async Task<Result<Tariff>> Handle(CreateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.ThemeId.HasValue)
            {
                var themeExists = await _themeRepository.GetByIdAsync(request.ThemeId.Value);
                if (themeExists == null)
                    return Result.Fail(new ThemeNotFoundError());
            }

            var tariff = new Tariff
            {
                Name = request.Name,
                Description = request.Description,
                PricePerMinute = request.PricePerMinute,
                BillingType = request.BillingType,
                ThemeId = request.ThemeId,
                IsActive = request.IsActive,
            };

            var created = await _repository.CreateAsync(tariff);

            if (created == null)
                return Result.Fail(new CreateFailedError());

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

