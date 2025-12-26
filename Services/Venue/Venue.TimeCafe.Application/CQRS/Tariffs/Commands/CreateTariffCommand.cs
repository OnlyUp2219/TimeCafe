namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record CreateTariffCommand(
    string Name,
    string? Description,
    decimal PricePerMinute,
    BillingType BillingType,
    string? ThemeId,
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
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название тарифа обязательно")
            .MaximumLength(100).WithMessage("Название не может превышать 100 символов");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Описание не может превышать 500 символов")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PricePerMinute)
            .GreaterThan(0).WithMessage("Цена за минуту должна быть больше 0");

        RuleFor(x => x.BillingType)
            .IsInEnum().WithMessage("Неверный тип биллинга");

        RuleFor(x => x.ThemeId)
            .Must(id => string.IsNullOrWhiteSpace(id) || (Guid.TryParse(id, out var guid) && guid != Guid.Empty))
            .WithMessage("Неверный идентификатор темы");
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
            Guid? themeId = null;
            if (!string.IsNullOrWhiteSpace(request.ThemeId))
            {
                themeId = Guid.Parse(request.ThemeId);
                var themeExists = await _themeRepository.GetByIdAsync(themeId.Value);
                if (themeExists == null)
                    return CreateTariffResult.ThemeNotFound();
            }

            var tariff = new Tariff
            {
                Name = request.Name,
                Description = request.Description,
                PricePerMinute = request.PricePerMinute,
                BillingType = request.BillingType,
                ThemeId = themeId,
                IsActive = request.IsActive,
            };

            var created = await _repository.CreateAsync(tariff);

            if (created == null)
                return CreateTariffResult.CreateFailed();

            return CreateTariffResult.CreateSuccess(created);
        }
        catch (Exception)
        {
            return CreateTariffResult.CreateFailed();
        }
    }
}
