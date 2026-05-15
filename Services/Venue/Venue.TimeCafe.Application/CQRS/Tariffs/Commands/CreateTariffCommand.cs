namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record CreateTariffCommand(
    string Name,
    string? Description,
    decimal PricePerMinute,
    BillingType BillingType,
    Guid? ThemeId,
    bool IsActive = true,
    string? Summary = null,
    List<string>? Features = null,
    List<string>? AudienceTags = null,
    int? MinSessionMinutes = null,
    string? RoundingRule = null,
    int? MaxGuests = null,
    string? CancellationPolicy = null,
    bool IsRecommended = false,
    int SortOrder = 0) : ICommand<Tariff>;

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

        RuleFor(x => x.MinSessionMinutes).ValidMinSessionMinutes();
        RuleFor(x => x.MaxGuests).ValidMaxGuests();
        RuleFor(x => x.RoundingRule).ValidRoundingRule();
        RuleFor(x => x.SortOrder).ValidSortOrder();
        RuleFor(x => x.Summary).ValidSummary();
        RuleFor(x => x.CancellationPolicy).ValidCancellationPolicy();
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
                Summary = request.Summary,
                Features = request.Features ?? [],
                AudienceTags = request.AudienceTags ?? [],
                MinSessionMinutes = request.MinSessionMinutes,
                RoundingRule = request.RoundingRule,
                MaxGuests = request.MaxGuests,
                CancellationPolicy = request.CancellationPolicy,
                IsRecommended = request.IsRecommended,
                SortOrder = request.SortOrder
            };

            var created = await _uow.Tariffs.CreateAsync(tariff, cancellationToken);

            if (created == null)
                return Result.Fail(new TariffCreateFailedError());

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

