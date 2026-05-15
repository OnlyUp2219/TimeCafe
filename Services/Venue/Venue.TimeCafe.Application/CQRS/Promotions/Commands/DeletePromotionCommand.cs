namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record DeletePromotionCommand(Guid PromotionId) : ICommand;

public class DeletePromotionCommandValidator : AbstractValidator<DeletePromotionCommand>
{
    public DeletePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class DeletePromotionCommandHandler(IUnitOfWork uow, IPublisher publisher) : ICommandHandler<DeletePromotionCommand>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IPublisher _publisher = publisher;

    public async Task<Result> Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _uow.Promotions.GetByIdAsync(request.PromotionId, cancellationToken);
            if (existing == null)
                return Result.Fail(new PromotionNotFoundError());

            var result = await _uow.Promotions.DeleteAsync(request.PromotionId, cancellationToken);

            if (!result)
                return Result.Fail(new PromotionDeleteFailedError());

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

