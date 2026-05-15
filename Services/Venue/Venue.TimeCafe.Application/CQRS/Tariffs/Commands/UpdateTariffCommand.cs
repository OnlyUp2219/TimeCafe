namespace Venue.TimeCafe.Application.CQRS.Tariffs.Commands;

public record UpdateTariffCommand(
    Guid TariffId, 
    string Name, 
    string Description, 
    decimal PricePerMinute, 
    BillingType BillingType, 
    Guid? ThemeId, 
    bool IsActive,
    string? Summary = null,
    List<string>? Features = null,
    List<string>? AudienceTags = null,
    int? MinSessionMinutes = null,
    string? RoundingRule = null,
    int? MaxGuests = null,
    string? CancellationPolicy = null,
    bool IsRecommended = false,
    int SortOrder = 0) : ICommand<Tariff>;

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

        RuleFor(x => x.MinSessionMinutes).ValidMinSessionMinutes();
        RuleFor(x => x.MaxGuests).ValidMaxGuests();
        RuleFor(x => x.RoundingRule).ValidRoundingRule();
        RuleFor(x => x.SortOrder).ValidSortOrder();
        RuleFor(x => x.Summary).ValidSummary();
        RuleFor(x => x.CancellationPolicy).ValidCancellationPolicy();
    }
}

public class UpdateTariffCommandHandler(IUnitOfWork uow, IMapper mapper, IPublisher publisher) : ICommandHandler<UpdateTariffCommand, Tariff>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IMapper _mapper = mapper;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Tariff>> Handle(UpdateTariffCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Tariffs.GetByIdAsync(request.TariffId, cancellationToken);
            if (existing == null)
                return Result.Fail(new TariffNotFoundError());

            var tariff = _mapper.Map<Tariff>(existing);
            _mapper.Map(request, tariff);

            if (request.ThemeId.HasValue)
            {
                var themeExists = await _uow.Themes.GetByIdAsync(request.ThemeId.Value, cancellationToken);
                if (themeExists == null)
                    return Result.Fail(new ThemeNotFoundError());
            }

            var updated = await _uow.Tariffs.UpdateAsync(tariff, cancellationToken);

            if (updated == null)
                return Result.Fail(new TariffUpdateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new TariffChangedEvent(updated.TariffId), cancellationToken);

            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

