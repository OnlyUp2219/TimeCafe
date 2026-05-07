namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record CreatePromotionCommand(
    string Name,
    string Description,
    decimal? DiscountPercent,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    PromotionType Type,
    Guid? TariffId = null,
    bool IsActive = true) : ICommand<Promotion>;

public class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name).ValidName("Название акции", 200);

        RuleFor(x => x.Description).ValidDescription(1000);

        RuleFor(x => x.DiscountPercent).ValidDiscountPercent()
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x.ValidFrom).ValidFromBeforeValidTo(x => x.ValidTo);
    }
}

public class CreatePromotionCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<CreatePromotionCommand, Promotion>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result<Promotion>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Type == PromotionType.Global && request.IsActive)
            {
                var activePromos = await _uow.Promotions.GetActiveAsync(cancellationToken);
                if (activePromos.Any(p => p.Type == PromotionType.Global))
                {
                    return Result.Fail(new ActiveGlobalPromotionAlreadyExistsError());
                }
            }

            var promotionResult = Promotion.Create(
                promotionId: null,
                name: request.Name,
                description: request.Description,
                validFrom: request.ValidFrom,
                validTo: request.ValidTo,
                isActive: request.IsActive,
                type: request.Type,
                tariffId: request.TariffId,
                DiscountPercent: request.DiscountPercent
            );

            if (promotionResult.IsFailed)
                return Result.Fail(promotionResult.Errors);

            var created = await _uow.Promotions.CreateAsync(promotionResult.Value, cancellationToken);

            if (created == null)
                return Result.Fail(new Error("Не удалось сохранить акцию в БД").WithMetadata("ErrorCode", "500"));

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new PromotionChangedEvent(created.PromotionId), cancellationToken);

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Не удалось создать акцию").CausedBy(ex).WithMetadata("ErrorCode", "500"));
        }
    }
}

