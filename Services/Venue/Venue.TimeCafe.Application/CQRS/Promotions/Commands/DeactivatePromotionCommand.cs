namespace Venue.TimeCafe.Application.CQRS.Promotions.Commands;

public record DeactivatePromotionCommand(Guid PromotionId) : ICommand;

public class DeactivatePromotionCommandValidator : AbstractValidator<DeactivatePromotionCommand>
{
    public DeactivatePromotionCommandValidator()
    {
        RuleFor(x => x.PromotionId).ValidGuidEntityId("Акция не найдена");
    }
}

public class DeactivatePromotionCommandHandler(IPromotionRepository repository) : ICommandHandler<DeactivatePromotionCommand>
{
    private readonly IPromotionRepository _repository = repository;

    public async Task<Result> Handle(DeactivatePromotionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(request.PromotionId);
            if (existing == null)
                return Result.Fail(new PromotionNotFoundError());

            var result = await _repository.DeactivateAsync(request.PromotionId);

            if (!result)
                return Result.Fail(new DeactivateFailedError());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("Внутренняя ошибка").CausedBy(ex));
        }
    }
}

