namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record DeactivatePromotionCommand(Guid PromotionId) : ICommand;

public class DeactivatePromotionCommandValidator : AbstractValidator<DeactivatePromotionCommand>
{
    public DeactivatePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class DeactivatePromotionCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeactivatePromotionCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(DeactivatePromotionCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _uow.Promotions.GetByIdAsync(request.PromotionId, cancellationToken);
            if (existing == null)
                return Result.Fail(new PromotionNotFoundError());

            var result = await _uow.Promotions.DeactivateAsync(request.PromotionId, cancellationToken);

            if (!result)
                return Result.Fail(new DeactivateFailedError());

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

