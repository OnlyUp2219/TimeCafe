namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record CreateTariffCommand(
    string Name,
    string? Description,
    decimal PricePerMinute,
    BillingType BillingType,
    Guid? ThemeId,
    bool IsActive = true) : IRequest<CreateTariffResult>;

public record CreateTariffResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    Tariff? Tariff = null) : ICqrsResultV2
{
    public static CreateTariffResult ThemeNotFound() =>
        new(false, Code: "ThemeNotFound", Message: "Тема не найдена", StatusCode: 404);

    public static CreateTariffResult CreateFailed() =>
        new(false, Code: "CreateTariffFailed", Message: "Не удалось создать тариф", StatusCode: 500);

    public static CreateTariffResult CreateSuccess(Tariff tariff) =>
        new(true, Message: "Тариф успешно создан", StatusCode: 201, Tariff: tariff);
}

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

public class CreateTariffCommandHandler(ITariffRepository repository, IThemeRepository themeRepository) : IRequestHandler<CreateTariffCommand, CreateTariffResult>
{
    private readonly ITariffRepository _repository = repository;
    private readonly IThemeRepository _themeRepository = themeRepository;

    public async Task<CreateTariffResult> Handle(CreateTariffCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.ThemeId.HasValue)
            {
                var themeExists = await _themeRepository.GetByIdAsync(request.ThemeId.Value);
                if (themeExists == null)
                    return CreateTariffResult.ThemeNotFound();
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
                return CreateTariffResult.CreateFailed();

            return CreateTariffResult.CreateSuccess(created);
        }
        catch (Exception ex)
        {
            throw new CqrsResultException(CreateTariffResult.CreateFailed(), ex);
        }
    }
}
