namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record ActivatePromotionCommand(Guid PromotionId) : ICommand;

public class ActivatePromotionCommandValidator : AbstractValidator<ActivatePromotionCommand>
{
    public ActivatePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class ActivatePromotionCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<ActivatePromotionCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(ActivatePromotionCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Promotions.GetByIdAsync(request.PromotionId, cancellationToken);
            if (existing == null)
                return Result.Fail(new PromotionNotFoundError());

            if (existing.Type == PromotionType.Global)
            {
                var activePromos = await _uow.Promotions.GetActiveAsync(cancellationToken);
                if (activePromos.Any(p => p.Type == PromotionType.Global && p.PromotionId != request.PromotionId))
                {
                    return Result.Fail(new ActiveGlobalPromotionAlreadyExistsError());
                }
            }

            var result = await _uow.Promotions.ActivateAsync(request.PromotionId, cancellationToken);

            if (!result)
                return Result.Fail(new ActivateFailedError());

            await _uow.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new PromotionChangedEvent(request.PromotionId), cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

