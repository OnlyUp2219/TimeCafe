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

public class CreateTariffCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<CreateTariffCommand, Tariff>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Tariff>> Handle(CreateTariffCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ThemeId.HasValue)
            {
                var themeExists = await _uow.Themes.GetByIdAsync(request.ThemeId.Value, cancellationToken);
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

            var created = await _uow.Tariffs.CreateAsync(tariff, cancellationToken);

            if (created == null)
                return Result.Fail(new CreateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new TariffChangedEvent(created.TariffId), cancellationToken);

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

